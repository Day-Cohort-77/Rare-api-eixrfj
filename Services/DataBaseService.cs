using Npgsql;
using RareAPI.Models;
using System.Data;

namespace RareAPI.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        private NpgsqlConnection CreateConnection()
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
            using var connection = new NpgsqlConnection(_connectionString.Replace("Database=rare", "Database=postgres"));
            await connection.OpenAsync();

            // Check if database exists
            using var checkCommand = new NpgsqlCommand(
                "SELECT 1 FROM pg_database WHERE datname = 'rare'",
                connection);
            var exists = await checkCommand.ExecuteScalarAsync();

            if (exists == null)
            {
                // Create the database
                using var createDbCommand = new NpgsqlCommand(
                    "CREATE DATABASE rare",
                    connection);
                await createDbCommand.ExecuteNonQueryAsync();
            }

            // Now connect to the harbormaster database and create tables
            string sql = File.ReadAllText("database-setup.sql");
            await ExecuteNonQueryAsync(sql);
        }

        // User-related methods
        public async Task<bool> UserExistsAsync(string email)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @email", connection);
            command.Parameters.AddWithValue("@email", email);

            var result = await command.ExecuteScalarAsync();
            return result != null && (long)result > 0;
        }

        public async Task<User?> CreateUserAsync(User newUser)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var insertSql = @"
                INSERT INTO Users (FirstName, LastName, Email, Password, CreatedOn, IsActive)
                VALUES (@firstName, @lastName, @email, @password, @createdOn, @isActive)
                RETURNING Id, FirstName, LastName, Email, CreatedOn, IsActive";

            using var command = new NpgsqlCommand(insertSql, connection);
            command.Parameters.AddWithValue("@firstName", newUser.FirstName);
            command.Parameters.AddWithValue("@lastName", newUser.LastName);
            command.Parameters.AddWithValue("@email", newUser.Email);
            command.Parameters.AddWithValue("@password", newUser.Password);
            command.Parameters.AddWithValue("@createdOn", DateTime.UtcNow);
            command.Parameters.AddWithValue("@isActive", true);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    CreatedOn = reader.GetDateTime(4),
                    IsActive = reader.GetBoolean(5)
                    // Don't return password
                };
            }

            return null;
        }

        public async Task<(int? userId, string? password)> GetUserCredentialsAsync(string email)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT Id, Password FROM Users WHERE Email = @email AND IsActive = true";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@email", email);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return (reader.GetInt32(0), reader.GetString(1));
            }

            return (null, null);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT Id, FirstName, LastName, Email, CreatedOn, IsActive FROM Users WHERE Id = @id";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    CreatedOn = reader.GetDateTime(4),
                    IsActive = reader.GetBoolean(5)
                    // Don't return password
                };
            }

            return null;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand("SELECT Id, FirstName, LastName, Email, CreatedOn, IsActive FROM Users WHERE IsActive = true", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    CreatedOn = reader.GetDateTime(4),
                    IsActive = reader.GetBoolean(5)
                    // Don't return password
                });
            }

            return users;
        }

        // Post-related methods
        public async Task<List<Post>> GetAllPostsAsync()
        {
            var posts = new List<Post>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand("SELECT Id, Title, Content, UserId, CreatedOn, UpdatedOn, IsPublished FROM Posts ORDER BY CreatedOn DESC", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                posts.Add(new Post
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Content = reader.GetString(2),
                    UserId = reader.GetInt32(3),
                    CreatedOn = reader.GetDateTime(4),
                    UpdatedOn = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    IsPublished = reader.GetBoolean(6)
                });
            }

            return posts;
        }

        public async Task<Post?> GetPostByIdAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT Id, Title, Content, UserId, CreatedOn, UpdatedOn, IsPublished FROM Posts WHERE Id = @id";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Post
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Content = reader.GetString(2),
                    UserId = reader.GetInt32(3),
                    CreatedOn = reader.GetDateTime(4),
                    UpdatedOn = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    IsPublished = reader.GetBoolean(6)
                };
            }

            return null;
        }

        public async Task<Post?> CreatePostAsync(Post newPost)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var insertSql = @"
                INSERT INTO Posts (Title, Content, UserId, CreatedOn, IsPublished)
                VALUES (@title, @content, @userId, @createdOn, @isPublished)
                RETURNING Id, Title, Content, UserId, CreatedOn, UpdatedOn, IsPublished";

            using var command = new NpgsqlCommand(insertSql, connection);
            command.Parameters.AddWithValue("@title", newPost.Title);
            command.Parameters.AddWithValue("@content", newPost.Content);
            command.Parameters.AddWithValue("@userId", newPost.UserId);
            command.Parameters.AddWithValue("@createdOn", DateTime.UtcNow);
            command.Parameters.AddWithValue("@isPublished", newPost.IsPublished);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Post
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Content = reader.GetString(2),
                    UserId = reader.GetInt32(3),
                    CreatedOn = reader.GetDateTime(4),
                    UpdatedOn = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    IsPublished = reader.GetBoolean(6)
                };
            }

            return null;
        }

        public async Task<Post?> UpdatePostAsync(int id, Post updatedPost)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var updateSql = @"
                UPDATE Posts
                SET Title = @title, Content = @content, UpdatedOn = @updatedOn, IsPublished = @isPublished
                WHERE Id = @id
                RETURNING Id, Title, Content, UserId, CreatedOn, UpdatedOn, IsPublished";

            using var command = new NpgsqlCommand(updateSql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@title", updatedPost.Title);
            command.Parameters.AddWithValue("@content", updatedPost.Content);
            command.Parameters.AddWithValue("@updatedOn", DateTime.UtcNow);
            command.Parameters.AddWithValue("@isPublished", updatedPost.IsPublished);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Post
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Content = reader.GetString(2),
                    UserId = reader.GetInt32(3),
                    CreatedOn = reader.GetDateTime(4),
                    UpdatedOn = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    IsPublished = reader.GetBoolean(6)
                };
            }

            return null;
        }

        public async Task<bool> DeletePostAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var deleteSql = "DELETE FROM Posts WHERE Id = @id";
            using var command = new NpgsqlCommand(deleteSql, connection);
            command.Parameters.AddWithValue("@id", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<Post>> GetPostsByUserIdAsync(int userId)
        {
            var posts = new List<Post>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT Id, Title, Content, UserId, CreatedOn, UpdatedOn, IsPublished FROM Posts WHERE UserId = @userId ORDER BY CreatedOn DESC";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                posts.Add(new Post
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Content = reader.GetString(2),
                    UserId = reader.GetInt32(3),
                    CreatedOn = reader.GetDateTime(4),
                    UpdatedOn = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    IsPublished = reader.GetBoolean(6)
                });
            }

            return posts;
        }
    }
}