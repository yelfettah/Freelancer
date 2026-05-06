using FreelanceFlow.Core.Models;

namespace FreelanceFlow.Core.Interfaces;

public interface IPdfGeneratorService
{
    byte[] GenerateProposalPdf(ProposalData data);
    byte[] GenerateContractPdf(ContractData data);
    byte[] GenerateInvoicePdf(InvoiceData data);
    byte[] GenerateFranchiseReportPdf(FranchiseReport data, FranchiseRequest request);
    byte[] GenerateFranchiseComparisonPdf(FranchiseCompareResult data);
    byte[] GenerateLawyerReportPdf(LawyerReport data, LawyerRequest request);
    byte[] GenerateRealEstateReportPdf(RealEstateReport data, RealEstateRequest request);
    byte[] GenerateRealEstateComparisonPdf(RealEstateCompareResult data);
    byte[] GenerateSMEQuotePdf(SMEQuoteReport data, SMEQuoteRequest request);
}
