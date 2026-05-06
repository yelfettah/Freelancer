using FreelanceFlow.Core.Entities;

namespace FreelanceFlow.Core.Interfaces;

public interface IConversationRepository
{
    Task<Conversation> GetByIdAsync(int id);
    Task<List<Conversation>> GetByUserIdAsync(Guid userId);
    Task<Conversation> CreateAsync(Conversation conversation);
    Task DeleteAsync(int id);
}
