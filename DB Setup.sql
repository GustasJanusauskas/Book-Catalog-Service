CREATE DATABASE bookcatalogdb;
\c bookcatalogdb;

CREATE TABLE users(
	usr_id bigserial UNIQUE NOT NULL,
	username varchar(128) UNIQUE NOT NULL,
	userpassword varchar(128) NOT NULL,
	email varchar(256) UNIQUE NOT NULL,
	created timestamptz,
    privileged boolean DEFAULT 'false',
	PRIMARY KEY(usr_id)
);

CREATE TABLE usersessions(
	usr_id bigint UNIQUE NOT NULL,
	sessionID varchar(64) UNIQUE NOT NULL
);

CREATE TABLE books(
    book_id bigserial UNIQUE NOT NULL,
	title varchar(512) NOT NULL,
	author varchar(512) NOT NULL,
	publisher varchar(512),
	published_date date DEFAULT '2000-01-01',
	genre varchar(128),
	ISBN varchar(20) NOT NULL,
    coverimage_path varchar (260),
	PRIMARY KEY(book_id)
);

CREATE TABLE book_status(
    book_id bigint UNIQUE NOT NULL,
    borrowed_by bigint DEFAULT -1,
    reserved_by bigint DEFAULT -1
);
