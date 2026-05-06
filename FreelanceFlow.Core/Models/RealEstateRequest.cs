namespace FreelanceFlow.Core.Models;

public class RealEstateRequest
{
    public string PropertyType { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public int SquareMeters { get; set; }
    public string RoomCount { get; set; } = "2+1";
    public int Floor { get; set; }
    public int TotalFloors { get; set; }
    public int BuildingAge { get; set; }
    public bool HasElevator { get; set; }
    public bool HasParking { get; set; }
    public bool HasBalcony { get; set; }
    public string HeatingType { get; set; } = string.Empty;
    public bool IsFurnished { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string AdditionalFeatures { get; set; } = string.Empty;
}
