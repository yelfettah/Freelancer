namespace FreelanceFlow.Core.Entities;

public class GeneratedDocument
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public DocumentType DocType { get; set; }
    public byte[] PdfData { get; set; } = Array.Empty<byte>();
    public string JsonData { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
}

public enum DocumentType
{
    Proposal = 0,
    Contract = 1,
    Invoice = 2
}
