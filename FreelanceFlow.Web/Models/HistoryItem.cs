namespace FreelanceFlow.Web.Models;

public class HistoryItem
{
    public string Module { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string GeneratedDocumentsJson { get; set; } = string.Empty;
}
