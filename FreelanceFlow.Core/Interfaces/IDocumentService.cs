using FreelanceFlow.Core.Models;

namespace FreelanceFlow.Core.Interfaces;

public interface IDocumentService
{
    Task<GeneratedDocuments> ProcessConversationAsync(ConversationInput input, Guid userId);
}
