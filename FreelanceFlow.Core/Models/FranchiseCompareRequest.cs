namespace FreelanceFlow.Core.Models;

public class FranchiseCompareRequest
{
    public FranchiseRequest Option1 { get; set; } = new();
    public FranchiseRequest Option2 { get; set; } = new();
}
