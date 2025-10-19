using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace UserRegistrationServer.data
{
    public class Context
    {
        private static string ServerConnectionString = string.Empty;
        private static string DatabaseName = "user_registration_server";
        private static string ConnectionString = string.Empty;

        public static void Init(IConfiguration config)
        {
            bool isTest = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";
            DatabaseName = isTest ? "user_registration_server_test" : "user_registration_server";
            string user = isTest ? config["TestDatabase:User"] : config["Database:User"];
            string pass = isTest ? config["TestDatabase:Password"] : config["Database:Password"];
            string host = isTest ? config["TestDatabase:Host"] : config["Database:Host"];

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
