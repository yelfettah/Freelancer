namespace FreelanceFlow.Core.Models;

public class DashboardStats
{
    public int TotalDocuments { get; set; }
    public decimal TotalProposalValue { get; set; }
    public int ThisMonthCount { get; set; }
    public decimal ThisMonthValue { get; set; }
    public Dictionary<string, int> ModuleUsage { get; set; } = new();
    public string Currency { get; set; } = "TL";
}
