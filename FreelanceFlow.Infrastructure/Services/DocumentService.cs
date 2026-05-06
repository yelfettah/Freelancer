using System.Text.Json;
using FreelanceFlow.Core.Entities;
using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Core.Models;

namespace FreelanceFlow.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly IClaudeAIService _claudeAiService;
    private readonly IPdfGeneratorService _pdfGeneratorService;
    private readonly IConversationRepository _conversationRepository;
    private readonly IDocumentRepository _documentRepository;

    public DocumentService(
        IClaudeAIService claudeAiService,
        IPdfGeneratorService pdfGeneratorService,
        IConversationRepository conversationRepository,
        IDocumentRepository documentRepository)
    {
        _claudeAiService = claudeAiService;
        _pdfGeneratorService = pdfGeneratorService;
        _conversationRepository = conversationRepository;
        _documentRepository = documentRepository;
    }

    public async Task<GeneratedDocuments> ProcessConversationAsync(ConversationInput input, Guid userId)
    {
        var conversation = new Conversation
        {
            UserId = userId,
            RawText = input.RawText,
            ClientName = input.ClientName,
            FreelancerName = input.FreelancerName,
            Sector = input.Sector,
            Currency = input.Currency,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        conversation = await _conversationRepository.CreateAsync(conversation);

        var generated = await _claudeAiService.AnalyzeConversationAsync(
            input.RawText,
            input.FreelancerName,
            input.ClientName,
            input.Sector,
            input.Currency);

        var proposalPdf = _pdfGeneratorService.GenerateProposalPdf(generated.Proposal);
        var contractPdf = _pdfGeneratorService.GenerateContractPdf(generated.Contract);
        var invoicePdf = _pdfGeneratorService.GenerateInvoicePdf(generated.Invoice);

        await _documentRepository.CreateAsync(new GeneratedDocument
        {
            ConversationId = conversation.Id,
            DocType = DocumentType.Proposal,
            PdfData = proposalPdf,
            JsonData = JsonSerializer.Serialize(generated.Proposal),
            CreatedAt = DateTime.UtcNow
        });

        await _documentRepository.CreateAsync(new GeneratedDocument
        {
            ConversationId = conversation.Id,
            DocType = DocumentType.Contract,
            PdfData = contractPdf,
            JsonData = JsonSerializer.Serialize(generated.Contract),
            CreatedAt = DateTime.UtcNow
        });

        await _documentRepository.CreateAsync(new GeneratedDocument
        {
            ConversationId = conversation.Id,
            DocType = DocumentType.Invoice,
            PdfData = invoicePdf,
            JsonData = JsonSerializer.Serialize(generated.Invoice),
            CreatedAt = DateTime.UtcNow
        });

        return generated;
    }
}
