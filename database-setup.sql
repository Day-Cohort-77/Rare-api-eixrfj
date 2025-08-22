-- Drop and recreate Users table with correct columns (dev only, this will delete all user data!)
DROP TABLE IF EXISTS Users CASCADE;
CREATE TABLE IF NOT EXISTS Users (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    bio VARCHAR(255),
    username VARCHAR(100) NOT NULL,
    password VARCHAR(255) NOT NULL,
    profile_image_url VARCHAR(255),
    created_on TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    active BOOLEAN NOT NULL DEFAULT true
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
        first_name,
        last_name,
        email,
        bio,
        username,
        password,
        profile_image_url,
        created_on,
        active
    )
VALUES (
        'John',
        'Doe',
        'john.doe@example.com',
        'Test bio for John',
        'johndoe',
        'password123',
        'https://example.com/john.jpg',
        CURRENT_TIMESTAMP,
        true
    ),
    (
        'Alice',
        'Smith',
        'alice@example.com',
        'Test bio for Alice',
        'alicesmith',
        'password',
        'https://example.com/alice.jpg',
        CURRENT_TIMESTAMP,
        true
    ) ON CONFLICT (email) DO NOTHING;
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