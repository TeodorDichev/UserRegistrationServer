using UserRegistrationServer.data;

namespace UserRegistrationServer.repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task UpdateFirstNameAsync(string email, string firstName);
        Task UpdateMiddleNameAsync(string email, string middleName);
        Task UpdateLastNameAsync(string email, string lastName);
        Task UpdatePasswordAsync(string email, string passwordHash);
    }
}
