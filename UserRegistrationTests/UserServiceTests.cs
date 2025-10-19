using Moq;
using UserRegistrationServer.data;
using UserRegistrationServer.repositories;
using UserRegistrationServer.services;

namespace UserRegistrationTests
{
    // we assume repo works so we mock it
    [TestClass]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _repoMock;
        private UserService _service;

        [TestInitialize]
        public void Setup()
        {
            _repoMock = new Mock<IUserRepository>();
            _service = new UserService(_repoMock.Object);
        }

        [TestMethod]
        public async Task RegisterAsyncShouldReturnErrorWhenEmailInvalid()
        {
            var data = new Dictionary<string, string>
            {
                { "email", "invalidemail" },
                { "password", "Password123!" },
                { "confirmPassword", "Password123!" },
                { "firstName", "test" },
                { "lastName", "test" },
                { "captcha", "true" }
            };

            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var result = await _service.RegisterAsync(data);

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.ContainsKey("email"));
        }

        [TestMethod]
        public async Task RegisterAsyncShouldFailWhenEmailAlreadyExists()
        {
            var data = new Dictionary<string, string>
            {
                { "email", "existing@example.com" },
                { "password", "Password123!" },
                { "confirmPassword", "Password123!" },
                { "firstName", "test" },
                { "lastName", "test" },
                { "captcha", "true" }
            };

            _repoMock.Setup(r => r.GetByEmailAsync("existing@example.com"))
                     .ReturnsAsync(new User { Email = "existing@example.com" });

            var result = await _service.RegisterAsync(data);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Email already exists", result.Errors["email"]);
        }

        [TestMethod]
        public async Task RegisterAsyncShouldSucceedWhenDataIsValid()
        {
            var data = new Dictionary<string, string>
            {
                { "email", "new@example.com" },
                { "password", "StrongPassword123!" },
                { "confirmPassword", "StrongPassword123!" },
                { "firstName", "John" },
                { "lastName", "Doe" },
                { "captcha", "true" }
            };

            _repoMock.Setup(r => r.GetByEmailAsync("new@example.com")).ReturnsAsync((User?)null);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var result = await _service.RegisterAsync(data);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Registration successful", result.Message);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        public async Task LoginAsyncShouldFailWhenInvalidCredentials()
        {
            _repoMock.Setup(r => r.GetByEmailAsync("wrong@example.com")).ReturnsAsync((User?)null);

            var data = new Dictionary<string, string>
            {
                { "email", "wrong@example.com" },
                { "password", "invalid" }
            };

            var result = await _service.LoginAsync(data);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Invalid credentials", result.Message);
        }

        [TestMethod]
        public async Task LoginAsyncShouldSucceedWhenCredentialsValid()
        {
            var hash = PasswordManager.Hash("Password123!");
            var user = new User { Email = "valid@example.com", PasswordHash = hash, FirstName = "test", LastName = "test" };

            _repoMock.Setup(r => r.GetByEmailAsync("valid@example.com")).ReturnsAsync(user);

            var data = new Dictionary<string, string>
            {
                { "email", "valid@example.com" },
                { "password", "Password123!" }
            };

            var result = await _service.LoginAsync(data);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Login successful", result.Message);
            Assert.AreEqual("test", result.User.FirstName);
            Assert.AreEqual("test", result.User.LastName);
        }

        [TestMethod]
        public async Task UpdateNamesAsyncShouldCallCorrectRepoMethods()
        {
            var data = new Dictionary<string, string>
            {
                { "email", "test@example.com" },
                { "firstName", "John" },
                { "middleName", "M" },
                { "lastName", "Doe" }
            };

            _repoMock.Setup(r => r.GetByEmailAsync("test@example.com"))
                     .ReturnsAsync(new User { Email = "test@example.com", FirstName = "John", LastName = "Doe" });

            await _service.UpdateNamesAsync(data);

            _repoMock.Verify(r => r.UpdateFirstNameAsync("test@example.com", "John"), Times.Once);
            _repoMock.Verify(r => r.UpdateMiddleNameAsync("test@example.com", "M"), Times.Once);
            _repoMock.Verify(r => r.UpdateLastNameAsync("test@example.com", "Doe"), Times.Once);
        }

        [TestMethod]
        public async Task UpdatePasswordAsyncShouldThrowWhenCurrentPasswordWrong()
        {
            var hash = PasswordManager.Hash("Correct123!");
            var user = new User { Email = "test@example.com", PasswordHash = hash };

            _repoMock.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

            var data = new Dictionary<string, string>
            {
                { "email", "test@example.com" },
                { "currentPassword", "WrongPass" },
                { "newPassword", "NewPass123!" }
            };

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _service.UpdatePasswordAsync(data);
            });
        }

        [TestMethod]
        public async Task UpdatePasswordAsyncShouldUpdateWhenValid()
        {
            var hash = PasswordManager.Hash("OldPass123!");
            var user = new User { Email = "test@example.com", PasswordHash = hash };

            _repoMock.SetupSequence(r => r.GetByEmailAsync("test@example.com"))
                .ReturnsAsync(user) // before update
                .ReturnsAsync(new User { Email = "test@example.com", PasswordHash = PasswordManager.Hash("NewPass123!") }); // after update

            var data = new Dictionary<string, string>
            {
                { "email", "test@example.com" },
                { "currentPassword", "OldPass123!" },
                { "newPassword", "NewPass123!" },
                { "confirmPassword", "NewPass123!" }
            };

            var result = await _service.UpdatePasswordAsync(data);

            _repoMock.Verify(r => r.UpdatePasswordAsync("test@example.com", It.IsAny<string>()), Times.Once);
            Assert.AreEqual("test@example.com", result.Email);
        }
    }
}
