namespace FreelanceFlow.Core.Models;

public class FranchiseReport
{
    public string BrandName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public decimal FranchiseFee { get; set; }
    public decimal TotalInvestment { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal MonthlyProfit { get; set; }
    public int ROIMonths { get; set; }
    public string PopulationAnalysis { get; set; } = string.Empty;
    public string CompetitionAnalysis { get; set; } = string.Empty;
    public string RiskAssessment { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string Currency { get; set; } = "TL";
}
