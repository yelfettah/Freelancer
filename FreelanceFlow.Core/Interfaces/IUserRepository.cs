using FreelanceFlow.Core.Entities;

namespace FreelanceFlow.Core.Interfaces;

public interface IUserRepository
{
    Task<User> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
}
