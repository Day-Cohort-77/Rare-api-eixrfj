using Npgsql;
using Rare.Models;
using System.Data;

namespace Rare.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'RareConnectionString' not found.");
        }

        public NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        // Helper method to execute non-query SQL commands
        public async Task ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(sql, connection);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            await command.ExecuteNonQueryAsync();
        }

        public async Task InitializeDatabaseAsync()
        {
            // First, create the database if it doesn't exist
            // Use lowercase for database name to avoid case sensitivity issues
            string dbName = "raredb";
            using var connection = new NpgsqlConnection(_connectionString.Replace("Database=raredb", "Database=postgres").Replace("Database=raredb", "Database=postgres"));
            await connection.OpenAsync();

            // Check if database exists
            using var checkCommand = new NpgsqlCommand(
                $"SELECT 1 FROM pg_database WHERE datname = '{dbName}'",
                connection);
            var exists = await checkCommand.ExecuteScalarAsync();

            if (exists == null)
            {
                // Create the database using lowercase name
                using var createDbCommand = new NpgsqlCommand(
                    $"CREATE DATABASE {dbName}",
                    connection);
                await createDbCommand.ExecuteNonQueryAsync();
            }

            // Now connect to the raredb database and create tables
            string sql = File.ReadAllText("database-setup.sql");
            await ExecuteNonQueryAsync(sql);
        }
        public async Task SeedDatabaseAsync()
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            // Check if users table has data
            using var command = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\"", connection);
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());

            if (count > 0)
            {
                return;
            }

            // Seed Users (let DB auto-generate id)
                        await ExecuteNonQueryAsync(
                                @"INSERT INTO ""Users"" (first_name, last_name, email, bio, username, password, profile_image_url, created_on, active)
                                    VALUES (@firstName, @lastName, @email, @bio, @username, @password, @profileImageUrl, NOW(), @active)",
                                new Dictionary<string, object>
                                {
                                        ["@firstName"] = "New",
                                        ["@lastName"] = "User",
                                        ["@email"] = "new_user@example.com",
                                        ["@bio"] = "Seeded user bio",
                                        ["@username"] = "new_user_name",
                                        ["@password"] = "hashed_password_string",
                                        ["@profileImageUrl"] = "some_url",
                                        ["@active"] = true
                                });

            // Get inserted user id
            int userId;
            using (var cmd = new NpgsqlCommand("SELECT id FROM \"Users\" WHERE username = @username", connection))
            {
                cmd.Parameters.AddWithValue("@username", "new_user_name");
                userId = (int)await cmd.ExecuteScalarAsync();
            }

            // Seed Posts
                        await ExecuteNonQueryAsync(
                                @"INSERT INTO ""Posts"" (user_id, title, publication_date, image_url, content, approved)
                                    VALUES (@userId, @title, NOW(), @imageUrl, @content, @approved)",
                                new Dictionary<string, object>
                                {
                                        ["@userId"] = userId,
                                        ["@title"] = "new_title",
                                        ["@imageUrl"] = "new_image_url",
                                        ["@content"] = "Seeded post content",
                                        ["@approved"] = true
                                });

            // Get inserted post id
            int postId;
            using (var cmd = new NpgsqlCommand("SELECT id FROM \"Posts\" WHERE title = @title", connection))
            {
                cmd.Parameters.AddWithValue("@title", "new_title");
                postId = (int)await cmd.ExecuteScalarAsync();
            }

            // Seed Comments
                        await ExecuteNonQueryAsync(
                                @"INSERT INTO ""Comments"" (post_id, author_id, content)
                                    VALUES (@postId, @authorId, @content)",
                                new Dictionary<string, object>
                                {
                                        ["@postId"] = postId,
                                        ["@authorId"] = userId,
                                        ["@content"] = "This is a seeded comment."
                                });
            await ExecuteNonQueryAsync(@"
                INSERT INTO users (id, username, email, password, created) 
                VALUES (@id, @username, @email, @password, NOW())",
                new Dictionary<string, object>
                {
                    ["@id"] = userId,
                    ["@username"] = "new_user_name",
                    ["@email"] = "new_user@example.com",
                    ["@password"] = "hashed_password_string"
                });

            // Seed Posts
            var posts = Guid.NewGuid().ToString();
            await ExecuteNonQueryAsync(@"
                INSERT INTO posts (id, userId, title, imageUrl, publicationDate) 
                VALUES (@id, @userId, @title, @imageUrl, NOW())",
                new Dictionary<string, object>
                {
                    ["@id"] = postId,
                    ["@userId"] = userId,
                    ["@title"] = "new_title",
                    ["@imageUrl"] = "new_image_url"
                });

            // Seed Comments
            var comments = Guid.NewGuid().ToString();
            await ExecuteNonQueryAsync(@"
                INSERT INTO comments (id, author_id, post_id, content, created) 
                VALUES (@id, @authorId, @postId, @content, NOW())",
                new Dictionary<string, object>
                {
                    ["@id"] = comments,
                    ["@authorId"] = userId,
                    ["@postId"] = postId,
                    ["@content"] = "This is a seeded comment."
                });
        }

        // Get all users
        public async Task<List<Users>> GetAllUsersAsync()
        {
            var users = new List<Users>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand("SELECT id, username, password, email FROM users", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new Users
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Email = reader.GetString(3)
                });
            }
            return users;
        }

        // Get users by ID
        public async Task<Users?> GetUsersByIdAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT id, username, password, email FROM users WHERE id = @id",
                connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Users
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Email = reader.GetString(3)
                };
            }

            return null;
        }

        // Get all Posts
        public async Task<List<Posts>> GetAllPostsAsync()
        {
            var posts = new List<Posts>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand("SELECT id, userId, title, imageUrl FROM posts", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                posts.Add(new Posts
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Title = reader.GetString(2),
                    ImageUrl = reader.GetString(3)
                });
            }
            return posts;
        }

        // Get Posts by id
        public async Task<Posts?> GetPostsByIdAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT id, userId, title, imageUrl FROM posts WHERE id = @id",
                connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Posts
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Title = reader.GetString(2),
                    ImageUrl = reader.GetString(3)
                };
            }

            return null;
        }
    }
}