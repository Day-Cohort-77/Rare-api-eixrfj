CREATE TABLE IF NOT EXISTS Users (
    Id SERIAL PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL,
    CreatedOn TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN NOT NULL DEFAULT true
);
-- Create Posts table for RareAPI
CREATE TABLE IF NOT EXISTS Posts (
    Id SERIAL PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Content TEXT NOT NULL,
    UserId INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    CreatedOn TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedOn TIMESTAMP NULL,
    IsPublished BOOLEAN NOT NULL DEFAULT false
);
-- Insert a test user (password is 'password123')
INSERT INTO Users (
        FirstName,
        LastName,
        Email,
        Password,
        CreatedOn,
        IsActive
    )
VALUES (
        'John',
        'Doe',
        'john.doe@example.com',
        'password123',
        CURRENT_TIMESTAMP,
        true
    );
-- Insert some test posts
INSERT INTO Posts (Title, Content, UserId, CreatedOn, IsPublished)
VALUES (
        'My First Post',
        'This is the content of my first post. It''s quite exciting!',
        1,
        CURRENT_TIMESTAMP,
        true
    ),
    (
        'Draft Post',
        'This is a draft post that hasn''t been published yet.',
        1,
        CURRENT_TIMESTAMP,
        false
    ),
    (
        'Another Published Post',
        'Here''s another post with some interesting content.',
        1,
        CURRENT_TIMESTAMP,
        true
    );