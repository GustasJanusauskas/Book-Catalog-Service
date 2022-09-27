# Description
A library book catalogue online service.
Submission for the BA IT challenge 2022 for Gustas Janušauskas.

# Features
A user can:
- Create an account
- Log in
- Browse books
- Search for a specific book by:
    - Title
    - Author
    - Publisher
    - Genre
    - ISBN code
    - Publishing date range
    - Reservation and borrowing availability
- Borrow a book (if available)
- Reserve a book (if book is borrowed and unreserved)
- Cancel a reservation

An administrator can:
- Do everything that a user can
- Mark books as returned (available for borrowing)
- Mark books as not reserved
- Users with a reservation get priority when the book is returned

# Setup
### Prerequisites
- Visual Studio 2022
- Node.js (npm)
- PostgreSQL

### Steps

1. Install prerequisites.
2. Navigate to the BookCatalogService\ClientApp folder and run:
```
npm i
```
3. To setup the database, run the commands included in 'DB Setup.sql' via postgreSQL.
4. To add the sample data, run the commands included in 'DB Sample Data.sql' via postgreSQL.
5. Enter PostgreSQL credentials in BookCatalogService\Controllers\APIController.cs
6. Run the webapp from Visual Studio, it should now be hosted at:
```
localhost:7056
```
7. Database comes with an admin account pre-created:
```
username: administrator
password: roooooot
```