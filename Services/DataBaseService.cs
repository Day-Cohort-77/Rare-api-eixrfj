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
            // Run table creation and seeding on the configured database
            using var connection = CreateConnection();
            await connection.OpenAsync();

            string sql = File.ReadAllText("database-setup.sql");
            await ExecuteNonQueryAsync(sql);
        }

        // User-related methods
        public async Task<bool> UserExistsAsync(string email)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand("SELECT COUNT(*) FROM Users WHERE email = @email", connection);
            command.Parameters.AddWithValue("@email", email);

            var result = await command.ExecuteScalarAsync();
            return result != null && (long)result > 0;
        }

        public async Task<User?> CreateUserAsync(User newUser)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var insertSql = @"
                INSERT INTO Users (first_name, last_name, email, bio, username, password, profile_image_url, created_on, active)
                VALUES (@first_name, @last_name, @email, @bio, @username, @password, @profile_image_url, @created_on, @active)
                RETURNING id, first_name, last_name, email, bio, username, password, profile_image_url, created_on, active";

            using var command = new NpgsqlCommand(insertSql, connection);
            command.Parameters.AddWithValue("@first_name", newUser.First_Name);
            command.Parameters.AddWithValue("@last_name", newUser.Last_Name);
            command.Parameters.AddWithValue("@email", newUser.Email);
            command.Parameters.AddWithValue("@bio", newUser.Bio);
            command.Parameters.AddWithValue("@username", newUser.Username);
            command.Parameters.AddWithValue("@password", newUser.Password);
            command.Parameters.AddWithValue("@profile_image_url", newUser.Profile_Image_Url);
            command.Parameters.AddWithValue("@created_on", DateTime.UtcNow);
            command.Parameters.AddWithValue("@active", newUser.Active);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    First_Name = reader.GetString(1),
                    Last_Name = reader.GetString(2),
                    Email = reader.GetString(3),
                    Bio = reader.GetString(4),
                    Username = reader.GetString(5),
                    Password = reader.GetString(6),
                    Profile_Image_Url = reader.GetString(7),
                    Created_On = reader.GetDateTime(8),
                    Active = reader.GetBoolean(9)
                };
            }

            return null;
        }

        public async Task<(int? userId, string? password)> GetUserCredentialsAsync(string email)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT id, password FROM Users WHERE email = @email AND active = true";
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

            var sql = "SELECT id, first_name, last_name, email, bio, username, password, profile_image_url, created_on, active FROM Users WHERE id = @id";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    First_Name = reader.GetString(1),
                    Last_Name = reader.GetString(2),
                    Email = reader.GetString(3),
                    Bio = reader.GetString(4),
                    Username = reader.GetString(5),
                    Password = reader.GetString(6),
                    Profile_Image_Url = reader.GetString(7),
                    Created_On = reader.GetDateTime(8),
                    Active = reader.GetBoolean(9)
                };
            }

            return null;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand("SELECT id, first_name, last_name, email, bio, username, password, profile_image_url, created_on, active FROM Users WHERE active = true", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    First_Name = reader.GetString(1),
                    Last_Name = reader.GetString(2),
                    Email = reader.GetString(3),
                    Bio = reader.GetString(4),
                    Username = reader.GetString(5),
                    Password = reader.GetString(6),
                    Profile_Image_Url = reader.GetString(7),
                    Created_On = reader.GetDateTime(8),
                    Active = reader.GetBoolean(9)
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