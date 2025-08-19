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
            _connectionString = configuration.GetConnectionString("RareConnectionString") ??
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
    }
}