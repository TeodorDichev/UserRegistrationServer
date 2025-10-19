using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MySql.Data.MySqlClient;
using UserRegistrationServer.data;
using UserRegistrationServer.repositories;
using UserRegistrationServer.services;

namespace UserRegistrationTests
{
    
    // db testing repo writes in real db
    [TestClass]
    public class UserRepositoryTests
    {
        private static UserRepository _repo;
        private  static string _testEmail;

        [ClassInitialize]
        public static void ClassSetup(TestContext testContext)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

            var builder = new ConfigurationBuilder().AddUserSecrets<UserRegistrationServer.Program>();
            var config = builder.Build();

            Context.Init(config);

            _repo = new UserRepository();
            _testEmail = "test@example.com";

            // Ensure test user exists before running tests
            var user = new User
            {
                FirstName = "test",
                MiddleName = "test",
                LastName = "test",
                Email = _testEmail,
                PasswordHash = "mockhashpassword"
            };

            _repo.AddAsync(user).GetAwaiter().GetResult();
        }

        [TestMethod]
        public async Task AddAsyncShouldInsertAndRetrieveUser()
        {
            string email = "testadd2@gmail.com";
            var user = new User
            {
                FirstName = "test",
                MiddleName = "test",
                LastName = "test",
                Email = email,
                PasswordHash = "mockhashpassword"
            };

            await _repo.AddAsync(user);
            var result = await _repo.GetByEmailAsync(email);

            Assert.IsNotNull(result);
            Assert.AreEqual("test", result.FirstName);
            Assert.AreEqual("test", result.LastName);
            Assert.AreEqual(email, result.Email);

            // clean up so can add it again in the future
            using var conn = Context.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM users WHERE email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.ExecuteNonQuery();
        }

        [TestMethod]
        public async Task UpdateFirstNameAsyncShouldChangeFirstName()
        {
            await _repo.UpdateFirstNameAsync(_testEmail, "new_name");
            string middleName = _repo.GetByEmailAsync(_testEmail).Result?.MiddleName ?? "";
            string lastName = _repo.GetByEmailAsync(_testEmail).Result?.LastName ?? "";

            var result = await _repo.GetByEmailAsync(_testEmail);

            Assert.IsNotNull(result);
            Assert.AreEqual("new_name", result.FirstName);
            Assert.AreEqual(middleName, result.MiddleName);
            Assert.AreEqual(lastName, result.LastName);
        }

        [TestMethod]
        public async Task UpdateMiddleNameAsyncShouldChangeMiddleName()
        {
            await _repo.UpdateMiddleNameAsync(_testEmail, "new_name");
            string firstName = _repo.GetByEmailAsync(_testEmail).Result?.FirstName ?? "";
            string lastName = _repo.GetByEmailAsync(_testEmail).Result?.LastName ?? "";

            var result = await _repo.GetByEmailAsync(_testEmail);

            Assert.IsNotNull(result);
            Assert.AreEqual("new_name", result.MiddleName);
            Assert.AreEqual(firstName, result.FirstName);
            Assert.AreEqual(lastName, result.LastName);
        }

        [TestMethod]
        public async Task UpdateLastNameAsyncShouldChangeLastName()
        {
            await _repo.UpdateLastNameAsync(_testEmail, "new_name");
            string middleName = _repo.GetByEmailAsync(_testEmail).Result?.MiddleName ?? "";
            string firstName = _repo.GetByEmailAsync(_testEmail).Result?.FirstName ?? "";

            var result = await _repo.GetByEmailAsync(_testEmail);

            Assert.IsNotNull(result);
            Assert.AreEqual("new_name", result.LastName);
            Assert.AreEqual(middleName, result.MiddleName);
            Assert.AreEqual(firstName, result.FirstName);
        }

        [TestMethod]
        public async Task UpdatePasswordAsyncShouldChangePassword()
        {
            string newHash = "newmockhashpassword";
            await _repo.UpdatePasswordAsync(_testEmail, newHash);
            var result = await _repo.GetByEmailAsync(_testEmail);

            Assert.IsNotNull(result);
            Assert.AreEqual(newHash, result.PasswordHash);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            using var conn = Context.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM users WHERE email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", _testEmail);
            cmd.ExecuteNonQuery();
        }
    }
}
