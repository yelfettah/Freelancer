namespace FreelanceFlow.Core.Models;

public class RealEstateCompareResult
{
    public RealEstateReport Report1 { get; set; } = new();
    public RealEstateReport Report2 { get; set; } = new();
    public RealEstateRequest Request1 { get; set; } = new();
    public RealEstateRequest Request2 { get; set; } = new();
    public string ComparisonSummary { get; set; } = string.Empty;
    public string BetterValue { get; set; } = string.Empty;
    public string BetterValueReason { get; set; } = string.Empty;
}
