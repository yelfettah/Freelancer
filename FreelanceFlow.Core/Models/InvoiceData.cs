namespace FreelanceFlow.Core.Models;

public class InvoiceData
{
    public string InvoiceNo { get; set; } = string.Empty;
    public string FreelancerName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public List<InvoiceItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal KDVRate { get; set; } = 0.20m;
    public decimal KDVAmount { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;

    public class InvoiceItem
    {
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
