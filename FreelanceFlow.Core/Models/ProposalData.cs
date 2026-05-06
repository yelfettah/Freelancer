namespace FreelanceFlow.Core.Models;

public class ProposalData
{
    public string ProjectTitle { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public List<string> Deliverables { get; set; } = new();
    public string Timeline { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
}
