namespace FreelanceFlow.Core.Models;

public class ContractData
{
    public string FreelancerName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public List<string> Clauses { get; set; } = new();
}
