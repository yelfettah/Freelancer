namespace FreelanceFlow.Core.Models;

public class SMEQuoteReport
{
    public string BusinessType { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal LaborCost { get; set; }
    public decimal MaterialCost { get; set; }
    public decimal TotalCost { get; set; }
    public List<string> Breakdown { get; set; } = new();
    public string Timeline { get; set; } = string.Empty;
    public string Warranty { get; set; } = string.Empty;
    public string Terms { get; set; } = string.Empty;
    public string Currency { get; set; } = "TL";
}
