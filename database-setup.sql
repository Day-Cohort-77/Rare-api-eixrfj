CREATE TABLE IF NOT EXISTS "Users" (
    "id" SERIAL PRIMARY KEY,
    "first_name" varchar,
    "last_name" varchar,
    "email" varchar,
    "bio" varchar,
    "username" varchar,
    "password" varchar,
    "profile_image_url" varchar,
    "created_on" date,
    "active" bit
);
CREATE TABLE IF NOT EXISTS "DemotionQueue" (
    "action" varchar,
    "admin_id" INTEGER,
    "approver_one_id" INTEGER,
    FOREIGN KEY("admin_id") REFERENCES "Users"("id"),
    FOREIGN KEY("approver_one_id") REFERENCES "Users"("id"),
    PRIMARY KEY (action, admin_id, approver_one_id)
);
CREATE TABLE IF NOT EXISTS "Subscriptions" (
    "id" SERIAL PRIMARY KEY,
    "follower_id" INTEGER,
    "author_id" INTEGER,
    "created_on" date,
    FOREIGN KEY("follower_id") REFERENCES "Users"("id"),
    FOREIGN KEY("author_id") REFERENCES "Users"("id")
);
CREATE TABLE IF NOT EXISTS "Posts" (
    "id" SERIAL PRIMARY KEY,
    "user_id" INTEGER,
    "category_id" INTEGER,
    "title" varchar,
    "publication_date" date,
    "image_url" varchar,
    "content" varchar,
    "approved" bit,
    FOREIGN KEY("user_id") REFERENCES "Users"("id")
);
CREATE TABLE IF NOT EXISTS "Comments" (
    "id" SERIAL PRIMARY KEY,
    "post_id" INTEGER,
    "author_id" INTEGER,
    "content" varchar,
    FOREIGN KEY("post_id") REFERENCES "Posts"("id"),
    FOREIGN KEY("author_id") REFERENCES "Users"("id")
);
CREATE TABLE IF NOT EXISTS "Reactions" (
    "id" SERIAL PRIMARY KEY,
    "label" varchar,
    "image_url" varchar
);
CREATE TABLE IF NOT EXISTS "PostReactions" (
    "id" SERIAL PRIMARY KEY,
    "user_id" INTEGER,
    "reaction_id" INTEGER,
    "post_id" INTEGER,
    FOREIGN KEY("user_id") REFERENCES "Users"("id"),
    FOREIGN KEY("reaction_id") REFERENCES "Reactions"("id"),
    FOREIGN KEY("post_id") REFERENCES "Posts"("id")
);
CREATE TABLE IF NOT EXISTS "Tags" (
    "id" SERIAL PRIMARY KEY,
    "label" varchar
);
CREATE TABLE IF NOT EXISTS "PostTags" (
    "id" SERIAL PRIMARY KEY,
    "post_id" INTEGER,
    "tag_id" INTEGER,
    FOREIGN KEY("post_id") REFERENCES "Posts"("id"),
    FOREIGN KEY("tag_id") REFERENCES "Tags"("id")
);
CREATE TABLE IF NOT EXISTS "Categories" (
    "id" SERIAL PRIMARY KEY,
    "label" varchar
);
INSERT INTO "Categories" ("label")
VALUES ('News');
INSERT INTO "Tags" ("label")
VALUES ('JavaScript');
INSERT INTO "Reactions" ("label", "image_url")
VALUES ('happy', 'https://pngtree.com/so/happy');