using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using UserRegistrationServer.data;

namespace UserRegistrationServer.repositories
{
    public class UserRepository : IUserRepository
    {
        public async Task<User?> GetByEmailAsync(string email)
        {
            using var conn = Context.GetConnection();
            await conn.OpenAsync();
            string query = "SELECT first_name, middle_name, last_name, email, password_hash FROM users WHERE email=@Email";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            var user = new User
            {
                FirstName = reader.GetString("first_name"),
                MiddleName = reader.GetString("middle_name"),
                LastName = reader.GetString("last_name"),
                Email = reader.GetString("email"),
                PasswordHash = reader.GetString("password_hash")
            };

            return user;
        }

        public async Task AddAsync(User user)
        {
            using var conn = Context.GetConnection();
            await conn.OpenAsync();
            string query = @"INSERT INTO users (first_name, middle_name, last_name, email, password_hash)
                         VALUES (@FirstName,@MiddleName,@LastName,@Email,@PasswordHash)";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
            cmd.Parameters.AddWithValue("@MiddleName", user.MiddleName ?? "");
            cmd.Parameters.AddWithValue("@LastName", user.LastName);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateFirstNameAsync(string email, string firstName)
        {
            using var conn = Context.GetConnection();
            await conn.OpenAsync();
            string query = @"UPDATE users SET first_name =@FirstName WHERE email=@Email";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@FirstName", firstName);
            cmd.Parameters.AddWithValue("@Email", email);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateMiddleNameAsync(string email, string middleName)
        {
            using var conn = Context.GetConnection();
            await conn.OpenAsync();
            string query = @"UPDATE users SET middle_name =@MiddleName WHERE email=@Email";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MiddleName", middleName);
            cmd.Parameters.AddWithValue("@Email", email);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateLastNameAsync(string email, string lastName)
        {
            using var conn = Context.GetConnection();
            await conn.OpenAsync();
            string query = @"UPDATE users SET last_name =@LastName WHERE email=@Email";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@LastName", lastName);
            cmd.Parameters.AddWithValue("@Email", email);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdatePasswordAsync(string email, string passwordHash)
        {
            using var conn = Context.GetConnection();
            await conn.OpenAsync();
            string query = "UPDATE users SET password_hash=@PasswordHash WHERE email=@Email";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
            cmd.Parameters.AddWithValue("@Email", email);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
