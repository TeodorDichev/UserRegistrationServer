using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserRegistrationTests
{
    [TestClass]
    public class PasswordManagerTests
    {
        [TestMethod]
        public void HashShouldReturnNonEmptyString()
        {
            string password = "Aa1@abc";
            string hash = UserRegistrationServer.services.PasswordManager.Hash(password);
            Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        }

        [TestMethod]
        public void VerifyShouldReturnTrueForCorrectPassword()
        {
            string password = "Aa1@abc";
            string hash = UserRegistrationServer.services.PasswordManager.Hash(password);
            bool result = UserRegistrationServer.services.PasswordManager.Verify(password, hash);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void VerifyShouldReturnFalseForIncorrectPassword()
        {
            string password = "Aa1@abc";
            string hash = UserRegistrationServer.services.PasswordManager.Hash(password);
            bool result = UserRegistrationServer.services.PasswordManager.Verify("Wrong123!", hash);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateShouldThrowForEmptyPassword()
        {
            Assert.Throws<ArgumentException>(() =>
                UserRegistrationServer.services.PasswordManager.Validate(""));
        }

        [TestMethod]
        public void ValidateShouldThrowForShortPassword()
        {
            Assert.Throws<ArgumentException>(() =>
                UserRegistrationServer.services.PasswordManager.Validate("Aa1@"));
        }

        [TestMethod]
        public void ValidateShouldThrowForMissingCharacterTypes()
        {
            Assert.Throws<ArgumentException>(() =>
                UserRegistrationServer.services.PasswordManager.Validate("aa1@aa"));

            Assert.Throws<ArgumentException>(() =>
                UserRegistrationServer.services.PasswordManager.Validate("AA1@AA"));

            Assert.Throws<ArgumentException>(() =>
                UserRegistrationServer.services.PasswordManager.Validate("Aa@aaa"));

            Assert.Throws<ArgumentException>(() =>
                UserRegistrationServer.services.PasswordManager.Validate("Aa1aaa"));
        }

        [TestMethod]
        public void ValidateShouldPassForValidPassword()
        {
            string valid = "Aa1@abc";
            UserRegistrationServer.services.PasswordManager.Validate(valid);
        }
    }
}