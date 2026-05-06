namespace FreelanceFlow.Core.Models;

public class GeneratedDocuments
{
    public ProposalData Proposal { get; set; } = new();
    public ContractData Contract { get; set; } = new();
    public InvoiceData Invoice { get; set; } = new();
    public string FollowUpEmail { get; set; } = string.Empty;
}
