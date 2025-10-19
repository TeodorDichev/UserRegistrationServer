using Microsoft.Extensions.Configuration;
using UserRegistrationServer.services;

namespace UserRegistrationTests
{
    [TestClass]
    public class JwtManagerTests
    {
        private string _testEmail = "test@example.com";

        [TestInitialize]
        public void Setup()
        {
            var builder = new ConfigurationBuilder().AddUserSecrets<UserRegistrationServer.Program>();
            var config = builder.Build();

            JwtManager.Init(config);
        }

        [TestMethod]
        public void GenerateAndValidateTokenShouldReturnEmail()
        {
            string token = JwtManager.GenerateToken(_testEmail, 1);
            string? email = JwtManager.ValidateToken(token);
            Assert.AreEqual(_testEmail, email);
        }

        [TestMethod]
        public void ValidateTokenShouldReturnNullIfExpired()
        {
            string token = JwtManager.GenerateToken(_testEmail, -1);
            string? email = JwtManager.ValidateToken(token);
            Assert.IsNull(email);
        }

        [TestMethod]
        public void ValidateTokenShouldReturnNullIfTampered()
        {
            string token = JwtManager.GenerateToken(_testEmail);
            string tampered = token.Replace('a', 'b');
            string? email = JwtManager.ValidateToken(tampered);
            Assert.IsNull(email);
        }
    }
}
