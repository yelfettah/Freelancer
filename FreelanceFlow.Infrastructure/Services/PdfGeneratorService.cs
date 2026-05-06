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

    public byte[] GenerateFranchiseReportPdf(FranchiseReport data, FranchiseRequest request)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));
                page.Header().Background("#0B3D91").Padding(15).Text("FRANCHISE ANALİZ RAPORU").FontColor(Colors.White).Bold().FontSize(18);
                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text($"Marka: {data.BrandName}").Bold();
                    col.Item().Text($"Konum: {data.City} / {data.District}");
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });
                        table.Cell().Element(CellStyle).Text("İsim Hakkı Ücreti");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{data.FranchiseFee:N2} {data.Currency}");
                        table.Cell().Element(CellStyle).Text("Toplam Yatırım");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{data.TotalInvestment:N2} {data.Currency}");
                        table.Cell().Element(CellStyle).Text("Tahmini Aylık Ciro");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{data.MonthlyRevenue:N2} {data.Currency}");
                        table.Cell().Element(CellStyle).Text("Tahmini Aylık Kar");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{data.MonthlyProfit:N2} {data.Currency}");
                        table.Cell().Element(CellStyle).Text("Geri Dönüş Süresi");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{data.ROIMonths} ay");
                    });
                    col.Item().Text("Nüfus ve Bölge Analizi").Bold().FontColor("#0B3D91");
                    col.Item().Text(data.PopulationAnalysis);
                    col.Item().Text("Rekabet Analizi").Bold().FontColor("#0B3D91");
                    col.Item().Text(data.CompetitionAnalysis);
                    col.Item().Text("Risk Değerlendirmesi").Bold().FontColor("#0B3D91");
                    col.Item().Text(data.RiskAssessment);
                    col.Item().Text("Öneri ve Değerlendirme").Bold().FontColor("#0B3D91");
                    col.Item().Text(data.Recommendation);
                    col.Item().Text($"Talep Bütçesi: {request.Budget:N2} {request.Currency}").Italic();
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateFranchiseComparisonPdf(FranchiseCompareResult data)
    {
        var winnerName = data.Winner == "Option2" ? data.Request2.BrandName : data.Request1.BrandName;
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(28);
                page.Size(PageSizes.A4);
                page.Header().Column(c =>
                {
                    c.Item().Text("FRANCHISE KARŞILAŞTIRMA RAPORU").Bold().FontSize(18);
                    c.Item().Text($"{data.Request1.BrandName} vs {data.Request2.BrandName}").FontSize(12);
                });
                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).Padding(8).Column(c =>
                        {
                            c.Item().Text("SEÇENEK 1").Bold();
                            c.Item().Text($"{data.Request1.BrandName} - {data.Request1.City}/{data.Request1.District}");
                            c.Item().Text($"Yatırım: {data.Report1.TotalInvestment:N0} {data.Report1.Currency}");
                            c.Item().Text($"Ciro: {data.Report1.MonthlyRevenue:N0}");
                            c.Item().Text($"Kar: {data.Report1.MonthlyProfit:N0}");
                            c.Item().Text($"ROI: {data.Report1.ROIMonths} ay");
                        });
                        row.ConstantItem(10);
                        row.RelativeItem().Border(1).Padding(8).Column(c =>
                        {
                            c.Item().Text("SEÇENEK 2").Bold();
                            c.Item().Text($"{data.Request2.BrandName} - {data.Request2.City}/{data.Request2.District}");
                            c.Item().Text($"Yatırım: {data.Report2.TotalInvestment:N0} {data.Report2.Currency}");
                            c.Item().Text($"Ciro: {data.Report2.MonthlyRevenue:N0}");
                            c.Item().Text($"Kar: {data.Report2.MonthlyProfit:N0}");
                            c.Item().Text($"ROI: {data.Report2.ROIMonths} ay");
                        });
                    });
                    col.Item().Text("Karşılaştırma Tablosu").Bold();
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); });
                        t.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("Kriter").Bold();
                            h.Cell().Element(CellStyle).Text("Seçenek 1").Bold();
                            h.Cell().Element(CellStyle).Text("Seçenek 2").Bold();
                            h.Cell().Element(CellStyle).Text("Fark").Bold();
                        });
                        AddComparisonRow(t, "İsim Hakkı", data.Report1.FranchiseFee, data.Report2.FranchiseFee, lowerIsBetter: true, data.Report1.Currency);
                        AddComparisonRow(t, "Toplam Yatırım", data.Report1.TotalInvestment, data.Report2.TotalInvestment, lowerIsBetter: true, data.Report1.Currency);
                        AddComparisonRow(t, "Aylık Ciro", data.Report1.MonthlyRevenue, data.Report2.MonthlyRevenue, lowerIsBetter: false, data.Report1.Currency);
                        AddComparisonRow(t, "Aylık Kar", data.Report1.MonthlyProfit, data.Report2.MonthlyProfit, lowerIsBetter: false, data.Report1.Currency);
                        AddComparisonRow(t, "Geri Dönüş (Ay)", data.Report1.ROIMonths, data.Report2.ROIMonths, lowerIsBetter: true, "");
                    });
                    col.Item().Text("Karşılaştırma Özeti").Bold();
                    col.Item().Text(data.ComparisonSummary);
                    col.Item().Background("#d1fae5").Padding(12).Text($"ÖNERİLEN: {winnerName}\n{data.WinnerReason}").Bold();
                });
            });
        });
        return document.GeneratePdf();
    }

    public byte[] GenerateLawyerReportPdf(LawyerReport data, LawyerRequest request)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(35);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));
                page.Header().Text("HUKUKİ SÜREÇ ANALİZ RAPORU").Bold().FontSize(18);
                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text($"Dava Türü: {data.CaseType}").Bold();
                    col.Item().Text($"Şehir: {data.City}");
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });
                        table.Cell().Element(CellStyle).Text("Avukatlık Ücreti (Min)");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{data.MinLawyerFee:N2} {data.Currency}");
                        table.Cell().Element(CellStyle).Text("Avukatlık Ücreti (Maks)");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{data.MaxLawyerFee:N2} {data.Currency}");
                        table.Cell().Element(CellStyle).Text("Mahkeme Harçları");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{data.CourtFees:N2} {data.Currency}");
                        table.Cell().Element(CellStyle).Text("Bilirkişi Ücreti");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{data.ExpertFees:N2} {data.Currency}");
                        table.Cell().Element(CellStyle).Text("Toplam Tahmini Maliyet");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{data.TotalEstimatedCost:N2} {data.Currency}");
                    });
                    col.Item().Text($"Tahmini Süre: {data.EstimatedDurationMonths} ay ({data.EstimatedDurationYears} yıl)").Bold();
                    col.Item().Text("Dava Süreci Adımları").Bold();
                    for (var i = 0; i < data.ProcessSteps.Count; i++)
                    {
                        col.Item().Text($"{i + 1}. {data.ProcessSteps[i]}");
                    }

                    col.Item().Text("Başarı Faktörleri").Bold();
                    foreach (var item in data.SuccessFactors)
                    {
                        col.Item().Text($"- {item}");
                    }

                    col.Item().Text("Yasal Dayanak").Bold();
                    col.Item().Text(data.LegalReferences);
                    col.Item().Text("Öneri ve Strateji").Bold();
                    col.Item().Text(data.Recommendation);
                    col.Item().Text($"Not: {request.Description}");
                });
            });
        });
        return document.GeneratePdf();
    }

    public byte[] GenerateRealEstateReportPdf(RealEstateReport data, RealEstateRequest request)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));
                page.Header().Text("EMLAK DEĞERLEME VE İLAN RAPORU").Bold().FontSize(18);
                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(3); });
                        table.Cell().Element(CellStyle).Text("Mülk Tipi");
                        table.Cell().Element(CellStyle).Text(request.PropertyType);
                        table.Cell().Element(CellStyle).Text("Konum");
                        table.Cell().Element(CellStyle).Text($"{request.City}/{request.District}/{request.Neighborhood}");
                        table.Cell().Element(CellStyle).Text("Metrekare");
                        table.Cell().Element(CellStyle).Text($"{request.SquareMeters} m2");
                        table.Cell().Element(CellStyle).Text("Oda Sayısı");
                        table.Cell().Element(CellStyle).Text(request.RoomCount);
                    });
                    col.Item().Text($"Tahmini Fiyat: {data.EstimatedPrice:N2} {data.Currency}").Bold().FontSize(20).FontColor("#0B3D91");
                    col.Item().Text($"Metrekare Birim Fiyat: {data.PricePerSqm:N2} {data.Currency}").Bold();
                    col.Item().Text("Piyasa Analizi").Bold();
                    col.Item().Text(data.MarketAnalysis);
                    col.Item().Text("Benzer İlanlarla Karşılaştırma").Bold();
                    col.Item().Text(data.ComparableListings);
                    col.Item().Text("Hedef Kitle").Bold();
                    col.Item().Text(data.TargetAudience);
                    col.Item().Text("Öneri").Bold();
                    col.Item().Text(data.Recommendation);
                });
            });

            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text("HAZIR İLAN METNİ").Bold().FontSize(18);
                    col.Item().Text(data.AdTitle).Bold().FontSize(14);
                    col.Item().Text(data.AdDescription);
                });
            });
        });
        return document.GeneratePdf();
    }

    public byte[] GenerateRealEstateComparisonPdf(RealEstateCompareResult data)
    {
        var better = data.BetterValue == "Option2" ? "Mülk 2" : "Mülk 1";
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(28);
                page.Size(PageSizes.A4);
                page.Header().Text("EMLAK KARŞILAŞTIRMA RAPORU").Bold().FontSize(18);
                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).Padding(8).Column(c =>
                        {
                            c.Item().Text("MÜLK 1").Bold();
                            c.Item().Text($"{data.Request1.PropertyType} - {data.Request1.City}/{data.Request1.District}");
                            c.Item().Text($"Fiyat: {data.Report1.EstimatedPrice:N0} {data.Report1.Currency}");
                            c.Item().Text($"m²: {data.Report1.PricePerSqm:N0}");
                        });
                        row.ConstantItem(10);
                        row.RelativeItem().Border(1).Padding(8).Column(c =>
                        {
                            c.Item().Text("MÜLK 2").Bold();
                            c.Item().Text($"{data.Request2.PropertyType} - {data.Request2.City}/{data.Request2.District}");
                            c.Item().Text($"Fiyat: {data.Report2.EstimatedPrice:N0} {data.Report2.Currency}");
                            c.Item().Text($"m²: {data.Report2.PricePerSqm:N0}");
                        });
                    });
                    col.Item().Text("Karşılaştırma Tablosu").Bold();
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); });
                        t.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("Kriter").Bold();
                            h.Cell().Element(CellStyle).Text("Mülk 1").Bold();
                            h.Cell().Element(CellStyle).Text("Mülk 2").Bold();
                            h.Cell().Element(CellStyle).Text("Fark").Bold();
                        });
                        AddComparisonRow(t, "Fiyat", data.Report1.EstimatedPrice, data.Report2.EstimatedPrice, true, data.Report1.Currency);
                        AddComparisonRow(t, "m² Fiyat", data.Report1.PricePerSqm, data.Report2.PricePerSqm, true, data.Report1.Currency);
                        AddTextRow(t, "Konum", $"{data.Request1.City}/{data.Request1.District}", $"{data.Request2.City}/{data.Request2.District}", "-");
                    });
                    col.Item().Text("Karşılaştırma Özeti").Bold();
                    col.Item().Text(data.ComparisonSummary);
                    col.Item().Background("#d1fae5").Padding(12).Text($"ÖNERİLEN: {better}\n{data.BetterValueReason}").Bold();
                });
            });
        });
        return document.GeneratePdf();
    }

    public byte[] GenerateSMEQuotePdf(SMEQuoteReport data, SMEQuoteRequest request)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));
                page.Header().Text("HİZMET TEKLİFİ").Bold().FontSize(18);
                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text($"İş Türü: {data.BusinessType}").Bold();
                    col.Item().Text($"Müşteri: {data.ClientName}");
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });
                        table.Cell().Element(CellStyle).Text("İşçilik");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{data.LaborCost:N2} {data.Currency}");
                        table.Cell().Element(CellStyle).Text("Malzeme");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{data.MaterialCost:N2} {data.Currency}");
                        table.Cell().Element(CellStyle).Text("Toplam");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{data.TotalCost:N2} {data.Currency}");
                    });
                    col.Item().Text("Detaylı Kalemler").Bold();
                    foreach (var item in data.Breakdown)
                    {
                        col.Item().Text($"- {item}");
                    }

                    col.Item().Text($"Tahmini Süre: {data.Timeline}");
                    col.Item().Text($"Garanti: {data.Warranty}");
                    col.Item().Text($"Ödeme Koşulları: {data.Terms}");
                    col.Item().Text($"İş Açıklaması: {request.JobDescription}");
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

    private static void AddComparisonRow(TableDescriptor table, string label, decimal value1, decimal value2, bool lowerIsBetter, string currency)
    {
        var option1Better = lowerIsBetter ? value1 <= value2 : value1 >= value2;
        var diff = value1 - value2;
        table.Cell().Element(CellStyle).Text(label);
        table.Cell().Element(c => CellStyle(c).Background(option1Better ? "#D5F5E3" : "#FADBD8")).Text($"{value1:N0} {currency}");
        table.Cell().Element(c => CellStyle(c).Background(option1Better ? "#FADBD8" : "#D5F5E3")).Text($"{value2:N0} {currency}");
        table.Cell().Element(CellStyle).Text($"{diff:+#,##0;-#,##0;0} {currency}");
    }

    private static void AddComparisonRow(TableDescriptor table, string label, int value1, int value2, bool lowerIsBetter, string suffix)
    {
        var option1Better = lowerIsBetter ? value1 <= value2 : value1 >= value2;
        var diff = value1 - value2;
        table.Cell().Element(CellStyle).Text(label);
        table.Cell().Element(c => CellStyle(c).Background(option1Better ? "#D5F5E3" : "#FADBD8")).Text($"{value1} {suffix}");
        table.Cell().Element(c => CellStyle(c).Background(option1Better ? "#FADBD8" : "#D5F5E3")).Text($"{value2} {suffix}");
        table.Cell().Element(CellStyle).Text($"{diff:+#;-#;0} {suffix}");
    }

    private static void AddTextRow(TableDescriptor table, string label, string value1, string value2, string diff)
    {
        table.Cell().Element(CellStyle).Text(label);
        table.Cell().Element(CellStyle).Text(value1);
        table.Cell().Element(CellStyle).Text(value2);
        table.Cell().Element(CellStyle).Text(diff);
    }
}
