namespace FreelanceFlow.Core.Entities;

public class Conversation
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string RawText { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string FreelancerName { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public User User { get; set; } = null!;
    public ICollection<GeneratedDocument> Documents { get; set; } = new List<GeneratedDocument>();
}
