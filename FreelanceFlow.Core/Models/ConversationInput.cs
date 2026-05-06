namespace FreelanceFlow.Core.Models;

public class ConversationInput
{
    public int Id { get; set; }
    public string RawText { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string FreelancerName { get; set; } = string.Empty;
    public string Sector { get; set; } = "Yazılım";
    public string Currency { get; set; } = "TL";
    public DateTime CreatedAt { get; set; }
}
