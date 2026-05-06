using FreelanceFlow.Core.Entities;
using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceFlow.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        return user ?? throw new InvalidOperationException($"Kullanıcı bulunamadı: {id}");
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }
}
