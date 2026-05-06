namespace FreelanceFlow.Core.Models;

public class FranchiseRequest
{
    public string BrandName { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public int Population { get; set; }
    public decimal Budget { get; set; }
    public string Currency { get; set; } = "TL";
    public string Notes { get; set; } = string.Empty;
}
