using System.Text.RegularExpressions;
using UserRegistrationServer.data;
using UserRegistrationServer.repositories;

namespace UserRegistrationServer.services
{
    public class UserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        public async Task<RegisterResult> RegisterAsync(Dictionary<string, string> data)
        {
            var result = new RegisterResult { Success = false, Errors = new Dictionary<string, string>() };

            string email = data.GetValueOrDefault("email", "").Trim();
            string password = data.GetValueOrDefault("password", "");
            string confirmPassword = data.GetValueOrDefault("confirmPassword", "");
            string firstName = data.GetValueOrDefault("firstName", "");
            string middleName = data.GetValueOrDefault("middleName", "");
            string lastName = data.GetValueOrDefault("lastName", "");
            string captcha = data.GetValueOrDefault("captcha", "false");

            if (captcha == "false")
                result.Errors["captcha"] = "Invalid captcha";

            if (string.IsNullOrWhiteSpace(email))
                result.Errors["email"] = "Email is required";
            else if (!EmailRegex.IsMatch(email))
                result.Errors["email"] = "Invalid email format";

            if (string.IsNullOrWhiteSpace(firstName))
                result.Errors["firstName"] = "First name is required";

            if (string.IsNullOrWhiteSpace(lastName))
                result.Errors["lastName"] = "Last name is required";

            if (string.IsNullOrWhiteSpace(email))
                result.Errors["email"] = "Email is required";

            if (string.IsNullOrWhiteSpace(password))
                result.Errors["password"] = "Password is required";

            if (password != confirmPassword)
                result.Errors["confirmPassword"] = "Passwords do not match";

            var existing = await _repo.GetByEmailAsync(email);
            if (existing != null)
                result.Errors["email"] = "Email already exists";

            try { PasswordManager.Validate(password); }
            catch (Exception ex) { result.Errors["password"] = ex.Message; }

            if (result.Errors.Count > 0)
            {
                result.Success = false;
                result.Message = "Please fix the errors below";
                return result;
            }

            var user = new User
            {
                Email = email,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                PasswordHash = PasswordManager.Hash(password)
            };

            await _repo.AddAsync(user);

            result.Success = true;
            result.Message = "Registration successful";
            result.User = user;
            return result;
        }

        public async Task<LoginResult> LoginAsync(Dictionary<string, string> data)
        {
            string email = data.GetValueOrDefault("email", "").Trim();
            string password = data.GetValueOrDefault("password", "");

            var result = new LoginResult();
            var user = await _repo.GetByEmailAsync(email);
            if (user == null || !PasswordManager.Verify(password, user.PasswordHash))
            {
                result.Success = false;
                result.Message = "Invalid credentials";
                return result;
            }


            result.Success = true;
            result.Message = "Login successful";
            result.User = new User
            {
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                Email = user.Email
            };

            return result;
        }

        public async Task<User> UpdateNamesAsync(Dictionary<string, string> data)
        {
            string email = data.GetValueOrDefault("email", "").Trim();
            string firstName = data.GetValueOrDefault("firstName", "");
            string middleName = data.GetValueOrDefault("middleName", "");
            string lastName = data.GetValueOrDefault("lastName", "");

            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(middleName) && string.IsNullOrEmpty(lastName))
                throw new Exception("Invalid new names!");

            if (!string.IsNullOrEmpty(firstName))
                await _repo.UpdateFirstNameAsync(email, firstName);
            if (!string.IsNullOrEmpty(middleName))
                await _repo.UpdateMiddleNameAsync(email, middleName);
            if (!string.IsNullOrEmpty(lastName))
                await _repo.UpdateLastNameAsync(email, lastName);

            var user = await _repo.GetByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found when updating names!");

            return user;
        }

        public async Task<User> UpdatePasswordAsync(Dictionary<string, string> data)
        {
            string email = data.GetValueOrDefault("email", "").Trim();
            string currentPassword = data.GetValueOrDefault("currentPassword", "");
            string newPassword = data.GetValueOrDefault("newPassword", "");
            string confirmPassword = data.GetValueOrDefault("confirmPassword", "");

            var user = await _repo.GetByEmailAsync(email);
            if (user == null )
                throw new Exception("User not found when updating password");
            if(!PasswordManager.Verify(currentPassword, user.PasswordHash))
                throw new Exception("Wrong password");
            if(newPassword != confirmPassword)
                throw new Exception("Passwords do not match");

            PasswordManager.Validate(newPassword);

            await _repo.UpdatePasswordAsync(email, PasswordManager.Hash(newPassword));

            return user;
        }

        internal async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _repo.GetByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found when updating names!");

            return user;
        }
    }
}
