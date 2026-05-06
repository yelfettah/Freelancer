using FreelanceFlow.Core.Models;

namespace FreelanceFlow.Core.Interfaces;

public interface IClaudeAIService
{
    Task<GeneratedDocuments> AnalyzeConversationAsync(
        string rawText,
        string freelancerName,
        string clientName,
        string sector,
        string currency);
}
