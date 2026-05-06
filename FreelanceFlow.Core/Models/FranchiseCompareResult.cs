namespace FreelanceFlow.Core.Models;

public class FranchiseCompareResult
{
    public FranchiseReport Report1 { get; set; } = new();
    public FranchiseReport Report2 { get; set; } = new();
    public FranchiseRequest Request1 { get; set; } = new();
    public FranchiseRequest Request2 { get; set; } = new();
    public string ComparisonSummary { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty;
    public string WinnerReason { get; set; } = string.Empty;
}
