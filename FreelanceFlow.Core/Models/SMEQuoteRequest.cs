namespace FreelanceFlow.Core.Models;

public class SMEQuoteRequest
{
    public string BusinessType { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public bool MaterialsIncluded { get; set; }
    public string UrgencyLevel { get; set; } = "Normal";
    public string Notes { get; set; } = string.Empty;
    public string Currency { get; set; } = "TL";
}
