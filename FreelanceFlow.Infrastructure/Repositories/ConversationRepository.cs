using FreelanceFlow.Core.Entities;
using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceFlow.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _dbContext;

    public ConversationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Conversation> GetByIdAsync(int id)
    {
        var conversation = await _dbContext.Conversations
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        return conversation ?? throw new InvalidOperationException($"Conversation bulunamadı: {id}");
    }

    public Task<List<Conversation>> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.Conversations
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Include(c => c.Documents)
            .ToListAsync();
    }

    public async Task<Conversation> CreateAsync(Conversation conversation)
    {
        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync();
        return conversation;
    }

    public async Task DeleteAsync(int id)
    {
        var conversation = await _dbContext.Conversations.FirstOrDefaultAsync(c => c.Id == id);
        if (conversation is null)
        {
            return;
        }

        conversation.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
    }
}
