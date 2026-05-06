using FreelanceFlow.Core.Entities;
using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceFlow.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _dbContext;

    public DocumentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GeneratedDocument> GetByIdAsync(int id)
    {
        var document = await _dbContext.GeneratedDocuments.FirstOrDefaultAsync(d => d.Id == id);
        return document ?? throw new InvalidOperationException($"Belge bulunamadı: {id}");
    }

    public Task<List<GeneratedDocument>> GetByConversationIdAsync(int conversationId)
    {
        return _dbContext.GeneratedDocuments
            .Where(d => d.ConversationId == conversationId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<GeneratedDocument> CreateAsync(GeneratedDocument document)
    {
        _dbContext.GeneratedDocuments.Add(document);
        await _dbContext.SaveChangesAsync();
        return document;
    }
}
