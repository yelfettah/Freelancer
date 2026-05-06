using FreelanceFlow.Core.Models;

namespace FreelanceFlow.Core.Interfaces;

public interface IPdfGeneratorService
{
    byte[] GenerateProposalPdf(ProposalData data);
    byte[] GenerateContractPdf(ContractData data);
    byte[] GenerateInvoicePdf(InvoiceData data);
}
