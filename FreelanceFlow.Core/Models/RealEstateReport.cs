namespace FreelanceFlow.Core.Models;

public class RealEstateReport
{
    public decimal EstimatedPrice { get; set; }
    public decimal PricePerSqm { get; set; }
    public string MarketAnalysis { get; set; } = string.Empty;
    public string ComparableListings { get; set; } = string.Empty;
    public string AdTitle { get; set; } = string.Empty;
    public string AdDescription { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string Currency { get; set; } = "TL";
}
