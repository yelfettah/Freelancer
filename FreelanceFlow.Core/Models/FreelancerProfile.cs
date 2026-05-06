namespace FreelanceFlow.Core.Models;

public class FreelancerProfile
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public decimal DailyRate { get; set; }
    public string Currency { get; set; } = "TL";
    public decimal KDVRate { get; set; } = 0.20m;
    public string BankName { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string Expertise { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
