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

            using var command = new NpgsqlCommand("SELECT id, user_id, category_id, title, publication_date, image_url, content, approved FROM Posts ORDER BY publication_date DESC", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                posts.Add(new Post
                {
                    Id = reader.GetInt32(0),
                    User_Id = reader.GetInt32(1),
                    Category_Id = reader.GetInt32(2),
                    Title = reader.GetString(3),
                    Publication_Date = reader.GetDateTime(4),
                    Image_Url = reader.GetString(5),
                    Content = reader.GetString(6),
                    Approved = reader.IsDBNull(7) ? (bool?)null : reader.GetBoolean(7)
                });
            }

            return posts;
        }

        public async Task<Post?> GetPostByIdAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT id, user_id, category_id, title, publication_date, image_url, content, approved FROM Posts WHERE id = @id";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Post
                {
                    Id = reader.GetInt32(0),
                    User_Id = reader.GetInt32(1),
                    Category_Id = reader.GetInt32(2),
                    Title = reader.GetString(3),
                    Publication_Date = reader.GetDateTime(4),
                    Image_Url = reader.GetString(5),
                    Content = reader.GetString(6),
                    Approved = reader.IsDBNull(7) ? (bool?)null : reader.GetBoolean(7)
                };
            }

            return null;
        }

        public async Task<Post?> CreatePostAsync(Post newPost)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var insertSql = @"
                INSERT INTO Posts (user_id, category_id, title, publication_date, image_url, content, approved)
                VALUES (@user_id, @category_id, @title, @publication_date, @image_url, @content, @approved)
                RETURNING id, user_id, category_id, title, publication_date, image_url, content, approved";

            using var command = new NpgsqlCommand(insertSql, connection);
            command.Parameters.AddWithValue("@user_id", newPost.User_Id);
            command.Parameters.AddWithValue("@category_id", newPost.Category_Id);
            command.Parameters.AddWithValue("@title", newPost.Title);
            command.Parameters.AddWithValue("@publication_date", newPost.Publication_Date);
            command.Parameters.AddWithValue("@image_url", newPost.Image_Url);
            command.Parameters.AddWithValue("@content", newPost.Content);
            command.Parameters.AddWithValue("@approved", (object?)newPost.Approved ?? DBNull.Value);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Post
                {
                    Id = reader.GetInt32(0),
                    User_Id = reader.GetInt32(1),
                    Category_Id = reader.GetInt32(2),
                    Title = reader.GetString(3),
                    Publication_Date = reader.GetDateTime(4),
                    Image_Url = reader.GetString(5),
                    Content = reader.GetString(6),
                    Approved = reader.IsDBNull(7) ? (bool?)null : reader.GetBoolean(7)
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
                SET user_id = @user_id, category_id = @category_id, title = @title, publication_date = @publication_date, image_url = @image_url, content = @content, approved = @approved
                WHERE id = @id
                RETURNING id, user_id, category_id, title, publication_date, image_url, content, approved";

            using var command = new NpgsqlCommand(updateSql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@user_id", updatedPost.User_Id);
            command.Parameters.AddWithValue("@category_id", updatedPost.Category_Id);
            command.Parameters.AddWithValue("@title", updatedPost.Title);
            command.Parameters.AddWithValue("@publication_date", updatedPost.Publication_Date);
            command.Parameters.AddWithValue("@image_url", updatedPost.Image_Url);
            command.Parameters.AddWithValue("@content", updatedPost.Content);
            command.Parameters.AddWithValue("@approved", (object?)updatedPost.Approved ?? DBNull.Value);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Post
                {
                    Id = reader.GetInt32(0),
                    User_Id = reader.GetInt32(1),
                    Category_Id = reader.GetInt32(2),
                    Title = reader.GetString(3),
                    Publication_Date = reader.GetDateTime(4),
                    Image_Url = reader.GetString(5),
                    Content = reader.GetString(6),
                    Approved = reader.IsDBNull(7) ? (bool?)null : reader.GetBoolean(7)
                };
            }

            return null;
        }

        public async Task<bool> DeletePostAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var deleteSql = "DELETE FROM Posts WHERE id = @id";
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

            var sql = "SELECT id, user_id, category_id, title, publication_date, image_url, content, approved FROM Posts WHERE user_id = @user_id ORDER BY publication_date DESC";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@user_id", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                posts.Add(new Post
                {
                    Id = reader.GetInt32(0),
                    User_Id = reader.GetInt32(1),
                    Category_Id = reader.GetInt32(2),
                    Title = reader.GetString(3),
                    Publication_Date = reader.GetDateTime(4),
                    Image_Url = reader.GetString(5),
                    Content = reader.GetString(6),
                    Approved = reader.IsDBNull(7) ? (bool?)null : reader.GetBoolean(7)
                });
            }

            return posts;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var Categories = new List<Category>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new NpgsqlCommand("SELECT id, Label FROM Categories ORDER BY Label ASC", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Categories.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    Label = reader.GetString(1),
                });
            }

            return Categories;
        }
        public async Task<Category?> CreateCategoryAsync(Category newCategory)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var insertSql = @"
                INSERT INTO Users (label)
                VALUES (@label)
                RETURNING id, label";

            using var command = new NpgsqlCommand(insertSql, connection);
            command.Parameters.AddWithValue("@label", newCategory.Label);


            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Category
                {
                    Id = reader.GetInt32(0),
                    Label = reader.GetString(1),

                };
            }

            return null;
        }
    }

}


