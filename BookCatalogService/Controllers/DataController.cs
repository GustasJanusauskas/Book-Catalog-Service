using Microsoft.AspNetCore.Mvc;
using Npgsql;
using CryptSharp;
using System.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Reflection.PortableExecutable;
using static BookCatalogService.Controllers.DataController;
using System.Data;

namespace BookCatalogService.Controllers {
    [ApiController]
    [Route("api")]
    public class DataController : ControllerBase, IDisposable {
        private readonly ILogger<DataController> _logger;
        private NpgsqlConnection conn;
        private Random rng = new Random();

        private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public DataController(ILogger<DataController> logger) {
            _logger = logger;

            //Connect to DB
            //Credentials go here
            string connString = "Host=localhost;Username=postgres;Password=root;Database=bookcatalogdb";

            conn = new NpgsqlConnection(connString);
            conn.Open();
        }

        public void Dispose() {
            //Closes DB connection
            conn.Close();
        }

        [HttpPost]
        [Route("userinfo")]
        async public Task<UserInfo> GetUserInfo([FromBody] StateRequest req) {
            await using (var cmd = new NpgsqlCommand("SELECT usr_id FROM usersessions WHERE sessionID = $1;", conn) {
                Parameters = {
                    new() { Value = req.session}
                }
            }) {
                object? res = await cmd.ExecuteScalarAsync();
                if (res == null) return new UserInfo(-1);
                else return new UserInfo((long)res);
            }
        }

        async public Task<long> GetIDFromSession(string session) {
            await using (var cmd = new NpgsqlCommand("SELECT usr_id FROM usersessions WHERE sessionID = $1;", conn) {
                Parameters = {
                    new() { Value = session}
                }
            }) {
                object? res = await cmd.ExecuteScalarAsync();
                if (res == null) return -1;
                else return (long)res;
            }
        }

        public class UserInfo {
            public long userid { get; set; }

            public UserInfo(long userid) { 
                this.userid = userid;
            }
        }

        [HttpPost]
        [Route("bookstatuschange")]
        async public Task ChangeBookState([FromBody] StateRequest req) {
            //SQL injection prevention
            string[] allowedColumns = new string[] {"borrowed_by","reserved_by"};
            if (!allowedColumns.Contains(req.column)) return;
            
            //Get user ID
            long userID = await GetIDFromSession(req.session);
            if (userID == -1) return;

            //Change book state
            if (req.state) {
                //Also checks if user has already borrowed or reserved the book, to prevent double-booking
                await using (var cmd = new NpgsqlCommand($"UPDATE book_status SET {req.column} = ( CASE WHEN {req.column} = CAST(-1 AS BIGINT) AND borrowed_by != $1 AND reserved_by != $1 THEN $1 ELSE {req.column} END) WHERE book_id = $2;", conn) {
                    Parameters = {
                    new() { Value = userID},
                    new() { Value = req.bookid}
                }
                }) {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            else {
                //Users can only remove reservations, not bookings (since they need to return the book first)
                if (req.column != "reserved_by") return;
                //Only removes the book status if user's ID is set (prevents freeing other users reservations and bookings)
                await using (var cmd = new NpgsqlCommand($"UPDATE book_status SET {req.column} = (CASE WHEN {req.column} = $2 THEN -1 ELSE {req.column} END) WHERE book_id = $1;", conn) {
                    Parameters = {
                    new() { Value = req.bookid},
                    new() { Value = userID}
                }
                }) {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class StateRequest {
            public string session { get; set; }
            public int bookid { get; set; }
            public string? column { get; set; }
            public bool state { get; set; }
        }

        [HttpGet]
        [Route("stats")]
        async public Task<Stats> GetStats() {
            var stats = new Stats();
            await using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM books;", conn)) {
                stats.bookCount = (long)await cmd.ExecuteScalarAsync();
            }
            return stats;
        }

        public class Stats {
            public long bookCount { get; set; } 
        }

        [HttpPost]
        [Route("adminbookstatuschange")]
        async public Task ChangeBookStateAdmin([FromBody] StateRequest req) {
            //SQL injection prevention
            string[] allowedColumns = new string[] { "borrowed_by", "reserved_by" };
            if (!allowedColumns.Contains(req.column)) return;

            //Get user ID
            long userID = await GetIDFromSession(req.session);
            if (userID == -1) return;

            //Verify user is privileged
            bool privileged;
            await using (var cmd = new NpgsqlCommand("SELECT privileged FROM users WHERE usr_id = $1;", conn) {
                Parameters = {
                        new() { Value = userID}
                }
            }) {
                privileged = (bool)await cmd.ExecuteScalarAsync();
            }
            if (!privileged) return;

            if (!req.state) {
                //Set book as available
                await using (var cmd = new NpgsqlCommand($"UPDATE book_status SET {req.column} = CAST(-1 AS BIGINT) WHERE book_id = $1;", conn) {
                    Parameters = {
                    new() { Value = req.bookid}
                }
                }) {
                    await cmd.ExecuteNonQueryAsync();
                }

                //If a user has reserved the book when it's been returned, automatically lend the book the reserver
                if (req.column == "borrowed_by") {
                    await using (var cmd = new NpgsqlCommand($"UPDATE book_status SET borrowed_by = reserved_by, reserved_by = -1 WHERE book_id = $1;", conn) {
                        Parameters = {
                            new() { Value = req.bookid}
                        }
                    }) {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        [HttpPost]
        [Route("adminlistings")]
        async public Task<ListingsResponse> GetAdminListings([FromBody] ListingRequest req) {
            ListingsResponse response = new ListingsResponse();
            List<Listing> listings = new List<Listing>();

            //Get user ID
            if (req.userSession == null) return response;
            long userID = await GetIDFromSession(req.userSession);
            if (userID == -1) return response;

            //Verify user is privileged
            bool privileged;
            await using (var cmd = new NpgsqlCommand("SELECT privileged FROM users WHERE usr_id = $1;", conn) {
                Parameters = {
                        new() { Value = userID}
                }
            }) {
                privileged = (bool)await cmd.ExecuteScalarAsync();
            }
            if (!privileged) return response;

            await using (var cmd = new NpgsqlCommand("SELECT books.*, count(*) OVER(), book_status.borrowed_by, book_status.reserved_by FROM books, book_status WHERE (book_status.borrowed_by != -1 OR book_status.reserved_by != -1) AND books.book_id = book_status.book_id ORDER BY title LIMIT 15 OFFSET $1;", conn) {
                Parameters = {
                        new() { Value = req.firstRecord}
                    }
            }) {
                await using (var reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        Listing newListing = new Listing();
                        newListing.ID = reader.GetInt64(0);
                        newListing.borrowed_by = reader.GetInt64(9);
                        newListing.reserved_by = reader.GetInt64(10);

                        newListing.book = BuildBook(reader);

                        response.total = reader.GetInt64(8);
                        listings.Add(newListing);
                    }
                }
            }

            response.listings = listings.ToArray();
            return response;
        }

        [HttpPost]
        [Route("listings")]
        async public Task<ListingsResponse> GetListings([FromBody] ListingRequest req) {
            ListingsResponse response = new ListingsResponse();
            List<Listing> listings = new List<Listing>();

            if (req.searchParams.paramsPresent) {

                //Format params
                if (req.searchParams.searchTitle == null) req.searchParams.searchTitle = "";
                if (req.searchParams.searchAuthor == null) req.searchParams.searchAuthor = "";
                if (req.searchParams.searchPublisher == null) req.searchParams.searchPublisher = "";
                if (req.searchParams.searchGenre == null) req.searchParams.searchGenre = "";
                if (req.searchParams.searchISBN == null) req.searchParams.searchISBN = "";
                if (req.searchParams.canBorrow == null) req.searchParams.canBorrow = false;
                if (req.searchParams.canReserve == null) req.searchParams.canReserve = false;

                req.searchParams.searchTitle += "%";
                req.searchParams.searchAuthor += "%";
                req.searchParams.searchPublisher += "%";
                req.searchParams.searchGenre += "%";
                req.searchParams.searchISBN += "%";

                //Build date params
                DateTime startDate;
                DateTime endDate;
                if (req.searchParams.searchDateStart != null && req.searchParams.searchDateEnd != null) {
                    string[] startSplit = req.searchParams.searchDateStart.Split('-');
                    string[] endSplit = req.searchParams.searchDateEnd.Split('-');

                    startDate = new DateTime(int.Parse(startSplit[0]), int.Parse(startSplit[1]), int.Parse(startSplit[2]));
                    endDate = new DateTime(int.Parse(endSplit[0]), int.Parse(endSplit[1]), int.Parse(endSplit[2]));
                }
                else {
                    startDate = DateTime.MinValue;
                    endDate = DateTime.MaxValue;
                }


                //Get book data
                await using (var cmd = new NpgsqlCommand("SELECT books.*, count(*) OVER(), book_status.borrowed_by, book_status.reserved_by FROM books, book_status WHERE title LIKE $2 AND author LIKE $3 AND publisher LIKE $4 AND genre LIKE $5 AND isbn LIKE $6 AND published_date >= $7 AND published_date < $8 AND (borrowed_by = -1 OR $9) AND (reserved_by = -1 OR $10) AND books.book_id = book_status.book_id ORDER BY title LIMIT 15 OFFSET $1;", conn) {
                    Parameters = {
                    new() { Value = req.firstRecord },
                    new() { Value = req.searchParams.searchTitle },
                    new() { Value = req.searchParams.searchAuthor },
                    new() { Value = req.searchParams.searchPublisher },
                    new() { Value = req.searchParams.searchGenre },
                    new() { Value = req.searchParams.searchISBN },
                    new() { Value = startDate },
                    new() { Value = endDate },
                    new() { Value = !req.searchParams.canBorrow },
                    new() { Value = !req.searchParams.canReserve },
                }
                }) {
                    await using (var reader = await cmd.ExecuteReaderAsync()) {
                        while (await reader.ReadAsync()) {
                            Listing newListing = new Listing();
                            newListing.ID = reader.GetInt64(0);
                            newListing.borrowed_by = reader.GetInt64(9);
                            newListing.reserved_by = reader.GetInt64(10);

                            newListing.book = BuildBook(reader);

                            response.total = reader.GetInt64(8);
                            listings.Add(newListing);
                        }
                    }
                }
            }
            else {
                await using (var cmd = new NpgsqlCommand("SELECT *, count(*) OVER() FROM books ORDER BY title LIMIT 15 OFFSET $1;", conn) {
                    Parameters = {
                        new() { Value = req.firstRecord}
                    }
                }) {
                    await using (var reader = await cmd.ExecuteReaderAsync()) {
                        while (await reader.ReadAsync()) {
                            Listing newListing = new Listing();
                            newListing.ID = reader.GetInt64(0);
                            newListing.book = BuildBook(reader);

                            response.total = reader.GetInt64(8);
                            listings.Add(newListing);
                        }
                    }
                }
            }

            response.listings = listings.ToArray();
            return response;
        }

        public class ListingsResponse {
            public Listing[] listings { get; set; }
            public long total { get; set; }

            public ListingsResponse() {
                listings = new Listing[] { };
                total = 0;
            }
        }

        [HttpPost]
        [Route("listing")]
        async public Task<Listing> GetListing([FromBody] ListingRequest req) {
            Listing result = new Listing();

            //Get listing data
            await using (var cmd = new NpgsqlCommand("SELECT books.*,book_status.borrowed_by, book_status.reserved_by FROM books,book_status WHERE books.book_id = $1 AND book_status.book_id = books.book_id;", conn) {
                Parameters = {
                    new() { Value = req.listingID}
                }
            }) {
                await using (var reader = await cmd.ExecuteReaderAsync()) {
                    if (!reader.HasRows) return new Listing();
                    await reader.ReadAsync();

                    result.ID = req.listingID;
                    result.book = BuildBook(reader);
                    result.borrowed_by = reader.GetInt64(8);
                    result.reserved_by = reader.GetInt64(9);
                }
            }

            return result;
        }

        private Book BuildBook(NpgsqlDataReader reader) {
            Book result = new Book();
            result.title = reader.GetString(1);
            result.author = reader.GetString(2);

            result.publisher = reader.GetString(3);
            result.dateOfPublishing = reader.GetDateTime(4).ToString("yyyy-MM-dd");
            result.genre = reader.GetString(5);
            result.ISBN = reader.GetString(6);
            result.imagePath = reader.GetString(7);

            return result;
        }

        public class ListingRequest {
            public long listingID { get; set; }
            public long? firstRecord { get; set; }
            public string? userSession { get; set; }
            public SearchParams? searchParams { get; set; }
        }

        public class SearchParams {
            public bool paramsPresent { get; set; }
            public string? searchTitle { get; set; }
            public string? searchAuthor { get; set; }
            public string? searchPublisher { get; set; }
            public string? searchGenre { get; set; }
            public string? searchISBN { get; set; }
            public string? searchDateStart { get; set; }
            public string? searchDateEnd { get; set; }
            public bool? canBorrow { get; set; } //Decides whether to check if book is borrowable, true = check, false = skip check
            public bool? canReserve { get; set; } //Decides whether to check if book is reservable.
        }

        public class Listing {
            public long ID { get; set; }
            public Book? book { get; set; }

            public long reserved_by { get; set; }
            public long borrowed_by { get; set; }
        }

        public class Book {
            public string title { get; set; }
            public string author { get; set; }
            public string publisher { get; set; }

            public string dateOfPublishing { get; set; }
            public string genre { get; set; }
            public string ISBN { get; set; }

            public string imagePath { get; set; }
        }

        [HttpPost]
        [Route("login")]
        async public Task<UserResponse> Login([FromBody] UserRequest? req) {
            //Verify data
            if (req == null) return new UserResponse("No account details specified.");
            if (req.user.Length > 256) return new UserResponse("Username/Email must be between 8 and 256 characters in length.");
            if (req.pass.Length > 128) return new UserResponse("Password must be between 8 and 128 characters in length.");

            string query;
            if (new EmailAddressAttribute().IsValid(req.user)) query = "SELECT * FROM users WHERE email = $1;";
            else query = "SELECT * FROM users WHERE username = $1;";

            //Check if password matches
            bool passMatches = false;
            long userID;
            bool privileged;
            await using (var cmd = new NpgsqlCommand(query, conn) {
                Parameters = {
                    new() { Value = req.user}
                } }) {
                await using (var reader = await cmd.ExecuteReaderAsync()) {
                    if (!reader.HasRows) return new UserResponse("Account not found.");
                    await reader.ReadAsync();

                    userID = reader.GetInt64(0);
                    privileged = reader.GetBoolean(5);
                    passMatches = reader.GetString(2) == Crypter.Blowfish.Crypt(req.pass, reader.GetString(2));
                }
            }

            if (passMatches) {
                //Generate session
                string session = "";
                for (int i = 0; i < 64; i++) session += CHARS[rng.Next(CHARS.Length)];

                //Save session to DB
                await using (var cmd = new NpgsqlCommand("UPDATE usersessions SET sessionID = $1 WHERE usr_id = $2;", conn) {
                    Parameters = {
                        new() {Value = session},
                        new() {Value = userID}
                    }
                }) {
                    await cmd.ExecuteNonQueryAsync();
                }

                var result = new UserResponse();
                result.privileged = privileged;
                result.session = session;
                return result;
            }
            return new UserResponse("Incorrect password.");
        }

        [HttpPost]
        [Route("register")]
        async public Task<UserResponse> Register([FromBody] UserRequest? req) {
            //Verify data
            if (req == null) return new UserResponse("No account details specified.");
            if (req.user.Length > 128 || req.user.Length < 8) return new UserResponse("Username must be between 8 and 128 characters in length.");
            if (req.pass.Length > 128 || req.pass.Length < 8) return new UserResponse("Password must be between 8 and 128 characters in length.");
            if (req.email == null || !new EmailAddressAttribute().IsValid(req.email) || req.email.Length > 256) return new UserResponse("Email must be valid.");

            //Hash password
            string hashedPass = Crypter.Blowfish.Crypt(req.pass); //bcrypt includes salt
            long userID;

            try {
                await using (var cmd = new NpgsqlCommand("INSERT INTO users (username,userpassword,email,created) VALUES (@u,@p,@e,NOW()) RETURNING usr_id;", conn)) {
                    cmd.Parameters.AddWithValue("u", req.user);
                    cmd.Parameters.AddWithValue("p", hashedPass);
                    cmd.Parameters.AddWithValue("e", req.email);
                    userID = (long) await cmd.ExecuteScalarAsync();
                }
            }
            catch ( PostgresException e) {
                Console.WriteLine(e.Message);
                switch (e.SqlState) {
                    case "23505":
                        return new UserResponse("Account with specified username or email already exists.");
                    default:
                        return new UserResponse("Error while creating account, please try again later.");
                }
            }

            //Generate session
            string session = "";
            for (int i = 0; i < 64; i++) session += CHARS[rng.Next(CHARS.Length)];
            await using (var cmd = new NpgsqlCommand("INSERT INTO usersessions (usr_id,sessionID) VALUES ($1,$2);", conn) {
                Parameters = {
                        new() {Value = userID},
                        new() {Value = session}
                    }
            }) {
                await cmd.ExecuteNonQueryAsync();
            }

            return new UserResponse();
        }

        public class UserRequest {
            public string? email { get; set; }
            public string user { get; set; }
            public string pass { get; set; }
        }

        public class UserResponse {
            public bool success { get; set; }
            public string? error { get; set; }
            public string? session { get; set; }
            public bool? privileged { get; set; }

            public UserResponse(string err) {
                error = err;
                success = false;
            }
            
            public UserResponse() {
                success = true;
            }
        }
    }
}