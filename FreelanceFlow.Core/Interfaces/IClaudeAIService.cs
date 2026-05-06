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

    Task<GeneratedDocuments> AnalyzeConversationWithProfileAsync(
        string rawText,
        string freelancerName,
        string clientName,
        string sector,
        string currency,
        FreelancerProfile profile,
        List<CustomParameter> parameters);

    Task<FranchiseReport> AnalyzeFranchiseAsync(FranchiseRequest request);
    Task<FranchiseCompareResult> CompareFranchisesAsync(FranchiseCompareRequest request);
    Task<LawyerReport> AnalyzeLawyerCaseAsync(LawyerRequest request);
    Task<RealEstateReport> AnalyzeRealEstateAsync(RealEstateRequest request);
    Task<RealEstateCompareResult> CompareRealEstateAsync(RealEstateCompareRequest request);
    Task<SMEQuoteReport> GenerateSMEQuoteAsync(SMEQuoteRequest request);
}
