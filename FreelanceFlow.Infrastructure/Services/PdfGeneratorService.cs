using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FreelanceFlow.Infrastructure.Services;

public class PdfGeneratorService : IPdfGeneratorService
{
    static PdfGeneratorService()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    public byte[] GenerateProposalPdf(ProposalData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Background("#1B4F72").Padding(15).Row(row =>
                {
                    row.RelativeItem().Text("FreelanceFlow").FontColor(Colors.White).Bold().FontSize(18);
                    row.ConstantItem(180).AlignRight().Text($"Tarih: {DateTime.Now:dd.MM.yyyy}").FontColor(Colors.White);
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Spacing(12);
                    col.Item().Text("TEKLİF DOKÜMANI").Bold().FontSize(20).FontColor("#1B4F72");
                    col.Item().LineHorizontal(1);
                    col.Item().Text($"Proje Başlığı: {data.ProjectTitle}").Bold();
                    col.Item().Text("Kapsam").Bold();
                    col.Item().Text(data.Scope);
                    col.Item().Text("Teslim Edilecekler").Bold();
                    foreach (var deliverable in data.Deliverables)
                    {
                        col.Item().Text($"- {deliverable}");
                    }

                    col.Item().Text($"Süre: {data.Timeline}");
                    col.Item().Text($"Fiyat: {data.Price:N2} {data.Currency}").Bold();
                    col.Item().Text($"Ödeme Koşulları: {data.PaymentTerms}");
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Sayfa ");
                    text.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateContractPdf(ContractData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(35);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("HİZMET SÖZLEŞMESİ").Bold().FontSize(20);
                    col.Item().PaddingTop(6).LineHorizontal(1);
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text("Taraflar").Bold().FontSize(14);
                    col.Item().Text($"Freelancer: {data.FreelancerName}");
                    col.Item().Text($"Müşteri: {data.ClientName}");

                    col.Item().PaddingTop(8).Text("Proje Detayları").Bold().FontSize(14);
                    col.Item().Text($"Proje Başlığı: {data.ProjectTitle}");
                    col.Item().Text($"Başlangıç Tarihi: {data.StartDate}");
                    col.Item().Text($"Bitiş Tarihi: {data.EndDate}");
                    col.Item().Text($"Toplam Tutar: {data.TotalPrice:N2} {data.Currency}");
                    col.Item().Text($"Ödeme Koşulları: {data.PaymentTerms}");

                    col.Item().PaddingTop(8).Text("Sözleşme Maddeleri").Bold().FontSize(14);
                    for (var i = 0; i < data.Clauses.Count; i++)
                    {
                        col.Item().Text($"{i + 1}. {data.Clauses[i]}");
                    }

                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("Freelancer İmza");
                            left.Item().Border(1).Height(60);
                            left.Item().PaddingTop(6).Text("Tarih: ____ / ____ / ______");
                        });
                        row.ConstantItem(20);
                        row.RelativeItem().Column(right =>
                        {
                            right.Item().Text("Müşteri İmza");
                            right.Item().Border(1).Height(60);
                            right.Item().PaddingTop(6).Text("Tarih: ____ / ____ / ______");
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateInvoicePdf(InvoiceData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("FATURA").Bold().FontSize(22);
                        row.RelativeItem().AlignRight().Text($"No: {data.InvoiceNo}").Bold();
                    });
                    col.Item().PaddingTop(6).LineHorizontal(1);
                });

                page.Content().PaddingVertical(16).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("Freelancer Bilgileri").Bold();
                            left.Item().Text(data.FreelancerName);
                            left.Item().Text($"Banka: {data.BankName}");
                            left.Item().Text($"IBAN: {data.IBAN}");
                        });
                        row.RelativeItem().AlignRight().Column(right =>
                        {
                            right.Item().Text("Müşteri Bilgileri").Bold();
                            right.Item().Text(data.ClientName);
                        });
                    });

                    col.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Açıklama").Bold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Miktar").Bold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Birim Fiyat").Bold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Toplam").Bold();
                        });

                        foreach (var item in data.Items)
                        {
                            table.Cell().Element(CellStyle).Text(item.Description);
                            table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice:N2} {data.Currency}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.Total:N2} {data.Currency}");
                        }
                    });

                    col.Item().AlignRight().Width(220).Column(total =>
                    {
                        total.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Ara Toplam:");
                            r.RelativeItem().AlignRight().Text($"{data.Subtotal:N2} {data.Currency}");
                        });
                        total.Item().Row(r =>
                        {
                            r.RelativeItem().Text($"KDV (%{data.KDVRate * 100:0}):");
                            r.RelativeItem().AlignRight().Text($"{data.KDVAmount:N2} {data.Currency}");
                        });
                        total.Item().LineHorizontal(1);
                        total.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Genel Toplam:").Bold();
                            r.RelativeItem().AlignRight().Text($"{data.Total:N2} {data.Currency}").Bold();
                        });
                    });

                    col.Item().PaddingTop(10).Text($"Son Ödeme Tarihi: {data.DueDate}").Bold();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .PaddingHorizontal(4);
    }
}
