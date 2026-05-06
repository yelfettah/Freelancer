namespace FreelanceFlow.Core.Models;

public class LawyerRequest
{
    public string CaseType { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OpponentType { get; set; } = "Bireysel";
    public decimal EstimatedValue { get; set; }
    public string Currency { get; set; } = "TL";
    public string Notes { get; set; } = string.Empty;
}
