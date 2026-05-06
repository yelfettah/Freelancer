using FreelanceFlow.Core.Entities;

namespace FreelanceFlow.Core.Interfaces;

public interface IDocumentRepository
{
    Task<GeneratedDocument> GetByIdAsync(int id);
    Task<List<GeneratedDocument>> GetByConversationIdAsync(int conversationId);
    Task<GeneratedDocument> CreateAsync(GeneratedDocument document);
}
