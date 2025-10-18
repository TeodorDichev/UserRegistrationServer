using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace UserRegistrationServer.data
{
    public class Context
    {
        private static readonly string ServerConnectionString;
        private static readonly string DatabaseName = "user_registration_server";
        private static readonly string ConnectionString;

        static Context()
        {
            var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
            var config = builder.Build();

            bool isTest = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";
            DatabaseName = isTest ? "user_registration_server_test" : "user_registration_server";
            string user = config["Database:User"] ?? "root";
            string pass = config["Database:Password"] ?? "root";
            string host = config["Database:Host"] ?? "localhost";

            if (isTest)
            {
                user = config["TestDatabase:User"] ?? "root";
                pass = config["TestDatabase:Password"] ?? "root";
                host = config["TestDatabase:Host"] ?? "localhost";
            }

            ServerConnectionString = $"Server={host};User={user};Password={pass};";
            ConnectionString = $"Server={host};Database={DatabaseName};User={user};Password={pass};";

            EnsureDatabaseExists();
            EnsureTablesExist();
        }

        private static void EnsureDatabaseExists()
        {
            using var conn = new MySqlConnection(ServerConnectionString);
            conn.Open();
            using var cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{DatabaseName}`;", conn);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Database '{DatabaseName}' ensured.");
        }

        private static void EnsureTablesExist()
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();

            string createUsersTable = @"
            CREATE TABLE IF NOT EXISTS users (
                id INT AUTO_INCREMENT PRIMARY KEY,
                first_name VARCHAR(100) NOT NULL,
                middle_name VARCHAR(100),
                last_name VARCHAR(100) NOT NULL,
                email VARCHAR(100) NOT NULL UNIQUE,
                password_hash VARCHAR(255) NOT NULL
            );";

            using var cmd = new MySqlCommand(createUsersTable, conn);
            cmd.ExecuteNonQuery();

            Console.WriteLine("Tables ensured.");
        }

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}
