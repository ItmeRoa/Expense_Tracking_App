using expense_tracker.Model;

namespace expense_tracker.Repo.IRepo;

public interface IUserRepo
{
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(int id, User user);
    Task DeleteUserAsync(int id);
    Task VerifiedEmail(User user);
    Task<User> GetUserByEmailAsync(string email);
    Task<bool> UserDoesExistByEmailAsync(string email);
    Task<User> GetUserByIdAsync(int id);
}