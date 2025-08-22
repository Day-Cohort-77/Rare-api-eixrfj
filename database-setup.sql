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
-- Drop Posts table if it exists (dev only, this will delete all post data!)
DROP TABLE IF EXISTS Posts CASCADE;
-- Create Posts table for RareAPI
CREATE TABLE IF NOT EXISTS Posts (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES Users(id) ON DELETE CASCADE,
    category_id INTEGER NOT NULL,
    title VARCHAR(255) NOT NULL,
    publication_date DATE NOT NULL,
    image_url VARCHAR(255),
    content VARCHAR(1000) NOT NULL,
    approved BOOLEAN
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
INSERT INTO Posts (
        user_id,
        category_id,
        title,
        publication_date,
        image_url,
        content,
        approved
    )
VALUES (
        1,
        1,
        'My First Post',
        '2025-08-22',
        'https://example.com/image1.jpg',
        'This is the content of my first post. It''s quite exciting!',
        true
    ),
    (
        1,
        2,
        'Draft Post',
        '2025-08-21',
        'https://example.com/image2.jpg',
        'This is a draft post that hasn''t been published yet.',
        false
    ),
    (
        1,
        1,
        'Another Published Post',
        '2025-08-20',
        'https://example.com/image3.jpg',
        'Here''s another post with some interesting content.',
        true
    ) ON CONFLICT DO NOTHING;