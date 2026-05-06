namespace FreelanceFlow.Core.Models;

public class RealEstateCompareRequest
{
    public RealEstateRequest Option1 { get; set; } = new();
    public RealEstateRequest Option2 { get; set; } = new();
}
