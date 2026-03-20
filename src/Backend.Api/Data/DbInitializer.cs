using Microsoft.Data.Sqlite;
using Dapper;
using System.Security.Cryptography;
using System.Text;
namespace Backend.Api.Data
{
    public class DbInitializer
    {
        private readonly string _connectionString;

        public DbInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);

            connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY,
                Username TEXT NOT NULL,
                PasswordHash TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Items (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL
            );
        ");

            var userCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Users");

            if (userCount == 0)
            {
                var hash = HashPassword("Admin123!");
                connection.Execute("INSERT INTO Users (Username, PasswordHash) VALUES (@u, @p)",
                    new { u = "admin", p = hash });
            }

            var itemCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Items");

            if (itemCount == 0)
            {
                for (int i = 1; i <= 5; i++)
                {
                    connection.Execute("INSERT INTO Items (Name) VALUES (@n)",
                        new { n = $"Item {i}" });
                }
            }
        }

        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }
}
