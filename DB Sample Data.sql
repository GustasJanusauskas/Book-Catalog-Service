\c bookcatalogdb;

--Admin account
WITH newid AS (
    INSERT INTO users(username,userpassword,email,created,privileged) VALUES ('administrator','$2a$06$AjyavQ3AIgWpHUl9bGA46uYDIbcnecikaQBDdOupWIMvdpK8a/6He','admin@email.com',NOW(),'true') RETURNING usr_id
)
INSERT INTO usersessions(usr_id,sessionID) VALUES ( (SELECT usr_id FROM newid),'EWI5lBZ1sRo2QiJ2pLmZsb2pdeCteLRFe3xRf10QCRjZwIWklrwp2Db8eser1GAp');

--Sample books
WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('A Tale of Two Cities','Charles Dickens','London: Chapman & Hall','1859-01-01','Historical novel','1-6640-0214-5','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('Where the Crawdads Sing','Delia Owens','Corsair','2018-11-08','General fiction','1-4540-0214-5','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('Harry Potter and the Philosophers Stone','J. K. Rowling','Bloomsbury','1997-06-26','Fantasy','0-7475-3269-9','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('And Then There Were None','Agatha Christie','Collins Crime Club','1939-11-06','Mistery','0-7974-3469-2','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('Dream of the Red Chamber','Cao Xueqin','Traditional','1791-01-01','Family saga','0-8974-2169-2','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('The Hobbit','J. R. R. Tolkien','George Allen & Unwin','1937-09-21','Fantasy','1-1973-2869-4','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('The Little Prince','Antoine de Saint-Exupéry','Reynal & Hitchcock','1943-04-01','Novella','0-1873-2469-3','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('The Lion, the Witch and the Wardrobe','C. S. Lewis','Geoffrey Bles','1950-10-16','Fantasy, Childrens fiction','0-0873-2469-2','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('She: A History of Adventure','H. Rider Haggard','Longmans','1887-01-01','Fantasy, Adventure, Romance','0-1473-2567-2','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('The Da Vinci Code','Dan Brown','Doubleday','2003-04-01','Mystery','0-385-50420-9','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('Harry Potter and the Chamber of Secrets','J. K. Rowling','Bloomsbury','1998-07-02','Fantasy','0-7475-3849-2','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('Harry Potter and the Prisoner of Azkaban','J. K. Rowling','Bloomsbury','1999-07-08','Fantasy','0-7475-4215-5','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('Harry Potter and the Goblet of Fire','J. K. Rowling','Bloomsbury','2000-07-08','Fantasy','0-7475-4624-1','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('Harry Potter and the Order of the Phoenix','J. K. Rowling','Bloomsbury','2003-06-21','Fantasy','0-7475-5100-6','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('Harry Potter and the Half-Blood Prince','J. K. Rowling','Bloomsbury','2005-07-16','Fantasy','0-7475-8108-8','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('Harry Potter and the Deathly Hallows','J. K. Rowling','Bloomsbury','2007-07-21','Fantasy','0-7475-9105-9','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );

WITH newid AS (
INSERT INTO books(title,author,publisher,published_date,genre,ISBN,coverimage_path) VALUES('The Catcher in the Rye','J. D. Salinger','Little, Brown and Company','1951-07-16','Coming-of-age','0-2476-9115-4','') RETURNING book_id
)
INSERT INTO book_status(book_id) VALUES ( (SELECT book_id FROM newid) );
