namespace FreelanceFlow.Core.Models;

public class LawyerReport
{
    public string CaseType { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal MinLawyerFee { get; set; }
    public decimal MaxLawyerFee { get; set; }
    public decimal CourtFees { get; set; }
    public decimal ExpertFees { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public int EstimatedDurationMonths { get; set; }
    public decimal EstimatedDurationYears { get; set; }
    public List<string> SuccessFactors { get; set; } = new();
    public List<string> ProcessSteps { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
    public string LegalReferences { get; set; } = string.Empty;
    public string Currency { get; set; } = "TL";
}
