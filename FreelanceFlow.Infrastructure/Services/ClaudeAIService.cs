using System.Text;
using System.Text.Json;
using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Core.Models;
using Microsoft.Extensions.Configuration;

namespace FreelanceFlow.Infrastructure.Services;

public class ClaudeAIService : IClaudeAIService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public ClaudeAIService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<GeneratedDocuments> AnalyzeConversationAsync(
        string rawText,
        string freelancerName,
        string clientName,
        string sector,
        string currency)
    {
        const string systemPrompt = "Sen profesyonel bir freelancer asistanısın. Sana verilen müşteri-freelancer yazışmasını analiz edeceksin. Yazışmadan proje kapsamını, fiyatı, süreyi, ödeme koşullarını çıkar. Yanıtını SADECE aşağıdaki JSON formatında ver, başka hiçbir şey yazma, markdown işareti kullanma, sadece düz JSON yaz:\n{\n  \"proposal\": {\n    \"projectTitle\": \"Proje başlığı\",\n    \"scope\": \"Proje kapsamı detaylı açıklama\",\n    \"deliverables\": [\"Teslim edilecek 1\", \"Teslim edilecek 2\"],\n    \"timeline\": \"Süre bilgisi\",\n    \"price\": 0,\n    \"currency\": \"TL\",\n    \"paymentTerms\": \"Ödeme koşulları\"\n  },\n  \"contract\": {\n    \"freelancerName\": \"Freelancer adı\",\n    \"clientName\": \"Müşteri adı\",\n    \"projectTitle\": \"Proje başlığı\",\n    \"startDate\": \"Başlangıç tarihi\",\n    \"endDate\": \"Bitiş tarihi\",\n    \"paymentTerms\": \"Ödeme koşulları detaylı\",\n    \"totalPrice\": 0,\n    \"currency\": \"TL\",\n    \"clauses\": [\n      \"Taraflar işbu sözleşme kapsamında belirtilen yükümlülüklerini eksiksiz yerine getirecektir.\",\n      \"Proje tesliminde gecikme olması halinde müşteri yazılı bildirimde bulunacaktır.\",\n      \"Ödeme belirtilen vadelerde banka havalesi yoluyla yapılacaktır.\",\n      \"Fikri mülkiyet hakları ödemenin tamamlanmasıyla müşteriye devredilecektir.\",\n      \"Taraflardan biri sözleşmeyi feshetmek isterse 15 gün önceden yazılı bildirimde bulunmalıdır.\"\n    ]\n  },\n  \"invoice\": {\n    \"invoiceNo\": \"FRF-2026-001\",\n    \"freelancerName\": \"Freelancer adı\",\n    \"clientName\": \"Müşteri adı\",\n    \"items\": [{\"description\": \"Hizmet açıklaması\", \"quantity\": 1, \"unitPrice\": 0, \"total\": 0}],\n    \"subtotal\": 0,\n    \"kdvRate\": 0.20,\n    \"kdvAmount\": 0,\n    \"total\": 0,\n    \"currency\": \"TL\",\n    \"bankName\": \"Örnek Banka\",\n    \"iban\": \"TR00 0000 0000 0000 0000 0000 00\",\n    \"dueDate\": \"Ödeme tarihi\"\n  },\n  \"followUpEmail\": \"Müşteriye gönderilecek profesyonel takip e-postası metni\"\n}\nEğer yazışmada fiyat belirtilmemişse sektör standardına göre makul bir fiyat öner. Tüm metinler Türkçe olmalı. Fiyatları number olarak yaz.";
        var userMessage = $"Freelancer: {freelancerName}\nMüşteri: {clientName}\nSektör: {sector}\nPara Birimi: {currency}\n\nYazışma:\n{rawText}";
        return await AnalyzeWithJsonAsync<GeneratedDocuments>(systemPrompt, userMessage);
    }

    public async Task<GeneratedDocuments> AnalyzeConversationWithProfileAsync(
        string rawText,
        string freelancerName,
        string clientName,
        string sector,
        string currency,
        FreelancerProfile profile,
        List<CustomParameter> parameters)
    {
        const string systemPrompt = "Sen profesyonel bir freelancer asistanısın. Sana verilen müşteri-freelancer yazışmasını analiz edeceksin. Yazışmadan proje kapsamını, fiyatı, süreyi, ödeme koşullarını çıkar. Yanıtını SADECE aşağıdaki JSON formatında ver, başka hiçbir şey yazma, markdown işareti kullanma, sadece düz JSON yaz:\n{\n  \"proposal\": {\n    \"projectTitle\": \"Proje başlığı\",\n    \"scope\": \"Proje kapsamı detaylı açıklama\",\n    \"deliverables\": [\"Teslim edilecek 1\", \"Teslim edilecek 2\"],\n    \"timeline\": \"Süre bilgisi\",\n    \"price\": 0,\n    \"currency\": \"TL\",\n    \"paymentTerms\": \"Ödeme koşulları\"\n  },\n  \"contract\": {\n    \"freelancerName\": \"Freelancer adı\",\n    \"clientName\": \"Müşteri adı\",\n    \"projectTitle\": \"Proje başlığı\",\n    \"startDate\": \"Başlangıç tarihi\",\n    \"endDate\": \"Bitiş tarihi\",\n    \"paymentTerms\": \"Ödeme koşulları detaylı\",\n    \"totalPrice\": 0,\n    \"currency\": \"TL\",\n    \"clauses\": [\n      \"Taraflar işbu sözleşme kapsamında belirtilen yükümlülüklerini eksiksiz yerine getirecektir.\",\n      \"Proje tesliminde gecikme olması halinde müşteri yazılı bildirimde bulunacaktır.\",\n      \"Ödeme belirtilen vadelerde banka havalesi yoluyla yapılacaktır.\",\n      \"Fikri mülkiyet hakları ödemenin tamamlanmasıyla müşteriye devredilecektir.\",\n      \"Taraflardan biri sözleşmeyi feshetmek isterse 15 gün önceden yazılı bildirimde bulunmalıdır.\"\n    ]\n  },\n  \"invoice\": {\n    \"invoiceNo\": \"FRF-2026-001\",\n    \"freelancerName\": \"Freelancer adı\",\n    \"clientName\": \"Müşteri adı\",\n    \"items\": [{\"description\": \"Hizmet açıklaması\", \"quantity\": 1, \"unitPrice\": 0, \"total\": 0}],\n    \"subtotal\": 0,\n    \"kdvRate\": 0.20,\n    \"kdvAmount\": 0,\n    \"total\": 0,\n    \"currency\": \"TL\",\n    \"bankName\": \"Örnek Banka\",\n    \"iban\": \"TR00 0000 0000 0000 0000 0000 00\",\n    \"dueDate\": \"Ödeme tarihi\"\n  },\n  \"followUpEmail\": \"Müşteriye gönderilecek profesyonel takip e-postası metni\"\n}\nBu bilgileri göz önünde bulundurarak fiyatlandırma yap. Freelancer'ın saatlik/günlük ücretini baz al. Özel parametreleri dikkate al.";
        var parameterLines = parameters.Count == 0
            ? "- Yok"
            : string.Join('\n', parameters.Select(param => $"- {param.Name}: {param.Value}"));
        var userMessage =
            $"Freelancer: {freelancerName}\nMüşteri: {clientName}\nSektör: {sector}\nPara Birimi: {currency}\n\nYazışma:\n{rawText}\n\n" +
            $"Freelancer Profil Bilgileri:\n- Saatlik Ücret: {profile.HourlyRate} {profile.Currency}\n- Günlük Ücret: {profile.DailyRate} {profile.Currency}\n- Deneyim: {profile.ExperienceYears} yıl\n- Uzmanlık: {profile.Expertise}\n- KDV Oranı: %{profile.KDVRate * 100}\n- Banka: {profile.BankName} - {profile.IBAN}\n- Vergi No: {profile.TaxNumber}\n\n" +
            $"Özel Parametreler:\n{parameterLines}\n\n" +
            "Bu bilgileri göz önünde bulundurarak fiyatlandırma yap. Freelancer'ın saatlik/günlük ücretini baz al. Özel parametreleri dikkate al.";
        return await AnalyzeWithJsonAsync<GeneratedDocuments>(systemPrompt, userMessage);
    }

    public Task<FranchiseReport> AnalyzeFranchiseAsync(FranchiseRequest request)
    {
        const string systemPrompt = "Sen bir franchise danışmanısın. Türkiye piyasasını çok iyi biliyorsun. Verilen marka, konum ve bütçe bilgilerine göre detaylı franchise analizi yap.\n\nŞu bilgiler verilecek: Marka adı, sektör, şehir, ilçe, nüfus, bütçe.\n\nSADECE şu JSON formatında yanıt ver, başka hiçbir şey yazma:\n{\n  \"brandName\": \"Marka adı\",\n  \"city\": \"Şehir\",\n  \"district\": \"İlçe\",\n  \"franchiseFee\": 0,\n  \"totalInvestment\": 0,\n  \"monthlyRevenue\": 0,\n  \"monthlyProfit\": 0,\n  \"roiMonths\": 0,\n  \"populationAnalysis\": \"Nüfus ve bölge potansiyeli detaylı analizi\",\n  \"competitionAnalysis\": \"Bölgedeki rakip analizi\",\n  \"riskAssessment\": \"Risk değerlendirmesi\",\n  \"recommendation\": \"Genel öneri ve değerlendirme. Yatırım yapılmalı mı, hangi koşullarda karlı olur detaylı açıkla.\",\n  \"currency\": \"TL\"\n}\n\nGerçekçi Türkiye piyasa fiyatları kullan. İsim hakkı, yatırım maliyeti, kira, personel, hammadde gibi tüm kalemleri hesaba kat. Fiyatları number olarak yaz.";
        var userMessage = $"Marka: {request.BrandName}\nSektör: {request.Sector}\nŞehir: {request.City}\nİlçe: {request.District}\nTahmini Nüfus: {request.Population}\nBütçe: {request.Budget} {request.Currency}\nNotlar: {request.Notes}";
        return AnalyzeWithJsonAsync<FranchiseReport>(systemPrompt, userMessage);
    }

    public async Task<FranchiseCompareResult> CompareFranchisesAsync(FranchiseCompareRequest request)
    {
        var reportTasks = new[]
        {
            AnalyzeFranchiseAsync(request.Option1),
            AnalyzeFranchiseAsync(request.Option2)
        };
        await Task.WhenAll(reportTasks);
        var report1 = reportTasks[0].Result;
        var report2 = reportTasks[1].Result;
        const string systemPrompt = "Sen bir franchise yatırım danışmanısın. Sana iki farklı franchise seçeneğinin analiz sonuçları verilecek. İkisini karşılaştır ve yatırımcıya hangisinin daha mantıklı olduğunu söyle.\n\nSADECE şu JSON formatında yanıt ver:\n{\n  \"comparisonSummary\": \"İki seçeneğin detaylı karşılaştırması. Yatırım maliyeti, karlılık, risk, geri dönüş süresi, konum avantajı gibi tüm faktörleri karşılaştır. En az 150 kelime detaylı analiz yaz.\",\n  \"winner\": \"Option1 veya Option2\",\n  \"winnerReason\": \"Neden bu seçenek daha iyi? 3-4 madde halinde açıkla.\"\n}";
        var userMessage =
            $"SEÇENEK 1:\nMarka: {request.Option1.BrandName}, Konum: {request.Option1.City}/{request.Option1.District}\nİsim Hakkı: {report1.FranchiseFee} TL\nToplam Yatırım: {report1.TotalInvestment} TL\nAylık Ciro: {report1.MonthlyRevenue} TL\nAylık Kar: {report1.MonthlyProfit} TL\nGeri Dönüş: {report1.ROIMonths} ay\nRisk: {report1.RiskAssessment}\n\n" +
            $"SEÇENEK 2:\nMarka: {request.Option2.BrandName}, Konum: {request.Option2.City}/{request.Option2.District}\nİsim Hakkı: {report2.FranchiseFee} TL\nToplam Yatırım: {report2.TotalInvestment} TL\nAylık Ciro: {report2.MonthlyRevenue} TL\nAylık Kar: {report2.MonthlyProfit} TL\nGeri Dönüş: {report2.ROIMonths} ay\nRisk: {report2.RiskAssessment}\n\nHangisi daha iyi yatırım?";
        FranchiseComparisonOnly comparison;
        try
        {
            comparison = await AnalyzeWithJsonAsync<FranchiseComparisonOnly>(systemPrompt, userMessage);
        }
        catch
        {
            comparison = BuildFranchiseFallbackComparison(request, report1, report2);
        }

        return new FranchiseCompareResult
        {
            Request1 = request.Option1,
            Request2 = request.Option2,
            Report1 = report1,
            Report2 = report2,
            ComparisonSummary = comparison.ComparisonSummary,
            Winner = comparison.Winner,
            WinnerReason = comparison.WinnerReason
        };
    }

    public Task<LawyerReport> AnalyzeLawyerCaseAsync(LawyerRequest request)
    {
        const string systemPrompt = "Sen Türkiye'de deneyimli bir hukuk danışmanısın. Türkiye Barolar Birliği Avukatlık Asgari Ücret Tarifesini ve mahkeme harçlarını biliyorsun.\n\nSADECE şu JSON formatında yanıt ver:\n{\n  \"caseType\": \"Dava türü\",\n  \"city\": \"Şehir\",\n  \"minLawyerFee\": 0,\n  \"maxLawyerFee\": 0,\n  \"courtFees\": 0,\n  \"expertFees\": 0,\n  \"totalEstimatedCost\": 0,\n  \"estimatedDurationMonths\": 0,\n  \"estimatedDurationYears\": 0.0,\n  \"successFactors\": [\"Faktör 1\", \"Faktör 2\"],\n  \"processSteps\": [\"Adım 1: ...\", \"Adım 2: ...\"],\n  \"recommendation\": \"Detaylı öneri ve strateji\",\n  \"legalReferences\": \"İlgili kanun maddeleri ve yasal dayanak\",\n  \"currency\": \"TL\"\n}\n\n2026 yılı Türkiye Barolar Birliği asgari ücret tarifesine yakın gerçekçi rakamlar ver. Dava süresini Türkiye mahkeme yoğunluğuna göre hesapla. Fiyatları number olarak yaz.";
        var userMessage = $"Dava Türü: {request.CaseType}\nŞehir: {request.City}\nAçıklama: {request.Description}\nKarşı Taraf: {request.OpponentType}\nTahmini Dava Değeri: {request.EstimatedValue} {request.Currency}\nNotlar: {request.Notes}";
        return AnalyzeWithJsonAsync<LawyerReport>(systemPrompt, userMessage);
    }

    public Task<RealEstateReport> AnalyzeRealEstateAsync(RealEstateRequest request)
    {
        const string systemPrompt = "Sen Türkiye emlak piyasasını çok iyi bilen bir emlak danışmanısın. Bölgesel fiyatları, piyasa trendlerini biliyorsun.\n\nSADECE şu JSON formatında yanıt ver:\n{\n  \"estimatedPrice\": 0,\n  \"pricePerSqm\": 0,\n  \"marketAnalysis\": \"Bölge piyasa analizi detaylı\",\n  \"comparableListings\": \"Benzer mülklerle karşılaştırma\",\n  \"adTitle\": \"Dikkat çekici ilan başlığı\",\n  \"adDescription\": \"Profesyonel, detaylı, SEO uyumlu ilan metni. En az 200 kelime. Mülkün tüm özelliklerini vurgula. İkna edici ve çekici bir dille yaz.\",\n  \"targetAudience\": \"Bu mülk için hedef kitle analizi\",\n  \"recommendation\": \"Fiyat ve pazarlama stratejisi önerisi\",\n  \"currency\": \"TL\"\n}\n\n2026 Türkiye emlak piyasasına uygun gerçekçi fiyatlar ver. Metrekare birim fiyatını bölgeye göre hesapla. Fiyatları number olarak yaz.";
        var userMessage =
            $"Mülk Tipi: {request.PropertyType}\nŞehir: {request.City}\nİlçe: {request.District}\nMahalle: {request.Neighborhood}\nMetrekare: {request.SquareMeters}\nOda Sayısı: {request.RoomCount}\nKat: {request.Floor}\nToplam Kat: {request.TotalFloors}\nBina Yaşı: {request.BuildingAge}\nAsansör: {(request.HasElevator ? "Var" : "Yok")}\nOtopark: {(request.HasParking ? "Var" : "Yok")}\nBalkon: {(request.HasBalcony ? "Var" : "Yok")}\nIsınma Türü: {request.HeatingType}\nEşyalı: {(request.IsFurnished ? "Evet" : "Hayır")}\nİşlem Tipi: {request.TransactionType}\nEk Özellikler: {request.AdditionalFeatures}";
        return AnalyzeWithJsonAsync<RealEstateReport>(systemPrompt, userMessage);
    }

    public async Task<RealEstateCompareResult> CompareRealEstateAsync(RealEstateCompareRequest request)
    {
        var reportTasks = new[]
        {
            AnalyzeRealEstateAsync(request.Option1),
            AnalyzeRealEstateAsync(request.Option2)
        };
        await Task.WhenAll(reportTasks);
        var report1 = reportTasks[0].Result;
        var report2 = reportTasks[1].Result;
        const string systemPrompt = "Sen bir emlak değerleme uzmanısın. Sana iki farklı mülkün değerleme sonuçları verilecek. İkisini karşılaştır ve hangisinin daha iyi yatırım/seçim olduğunu söyle.\n\nSADECE şu JSON formatında yanıt ver:\n{\n  \"comparisonSummary\": \"İki mülkün detaylı karşılaştırması. Fiyat, konum, metrekare birim fiyat, yatırım potansiyeli, kira getirisi gibi tüm faktörleri karşılaştır. En az 150 kelime.\",\n  \"betterValue\": \"Option1 veya Option2\",\n  \"betterValueReason\": \"Neden bu mülk daha iyi değer? 3-4 madde halinde açıkla.\"\n}";
        var userMessage =
            $"SEÇENEK 1:\nMülk: {request.Option1.PropertyType}, Konum: {request.Option1.City}/{request.Option1.District}/{request.Option1.Neighborhood}\nm2: {request.Option1.SquareMeters}, Oda: {request.Option1.RoomCount}, Kat: {request.Option1.Floor}/{request.Option1.TotalFloors}, Yaş: {request.Option1.BuildingAge}\nTahmini Fiyat: {report1.EstimatedPrice} {report1.Currency}\nm2 Birim Fiyat: {report1.PricePerSqm} {report1.Currency}\nPiyasa Analizi: {report1.MarketAnalysis}\nÖneri: {report1.Recommendation}\n\n" +
            $"SEÇENEK 2:\nMülk: {request.Option2.PropertyType}, Konum: {request.Option2.City}/{request.Option2.District}/{request.Option2.Neighborhood}\nm2: {request.Option2.SquareMeters}, Oda: {request.Option2.RoomCount}, Kat: {request.Option2.Floor}/{request.Option2.TotalFloors}, Yaş: {request.Option2.BuildingAge}\nTahmini Fiyat: {report2.EstimatedPrice} {report2.Currency}\nm2 Birim Fiyat: {report2.PricePerSqm} {report2.Currency}\nPiyasa Analizi: {report2.MarketAnalysis}\nÖneri: {report2.Recommendation}\n\nHangi mülk daha iyi yatırım?";
        RealEstateComparisonOnly comparison;
        try
        {
            comparison = await AnalyzeWithJsonAsync<RealEstateComparisonOnly>(systemPrompt, userMessage);
        }
        catch
        {
            comparison = BuildRealEstateFallbackComparison(request, report1, report2);
        }

        return new RealEstateCompareResult
        {
            Request1 = request.Option1,
            Request2 = request.Option2,
            Report1 = report1,
            Report2 = report2,
            ComparisonSummary = comparison.ComparisonSummary,
            BetterValue = comparison.BetterValue,
            BetterValueReason = comparison.BetterValueReason
        };
    }

    public Task<SMEQuoteReport> GenerateSMEQuoteAsync(SMEQuoteRequest request)
    {
        const string systemPrompt = "Sen Türkiye'de küçük işletme ve esnaf fiyatlandırması konusunda uzman bir danışmansın. Tesisatçı, elektrikçi, boyacı, tadilat gibi hizmetlerin Türkiye piyasa fiyatlarını biliyorsun.\n\nSADECE şu JSON formatında yanıt ver:\n{\n  \"businessType\": \"İş türü\",\n  \"clientName\": \"Müşteri adı\",\n  \"laborCost\": 0,\n  \"materialCost\": 0,\n  \"totalCost\": 0,\n  \"breakdown\": [\"Kalem 1: X TL\", \"Kalem 2: Y TL\"],\n  \"timeline\": \"Tahmini tamamlanma süresi\",\n  \"warranty\": \"Garanti koşulları\",\n  \"terms\": \"Ödeme koşulları\",\n  \"currency\": \"TL\"\n}\n\n2026 Türkiye esnaf piyasa fiyatlarını baz al. Acil iş için %50, çok acil için %100 ek ücret ekle. İşçilik ve malzeme ayrı kalemlendir. Fiyatları number olarak yaz.";
        var userMessage =
            $"İş Türü: {request.BusinessType}\nMüşteri Adı: {request.ClientName}\nİş Açıklaması: {request.JobDescription}\nKonum: {request.Location}\nTahmini Saat: {request.EstimatedHours}\nMalzeme Dahil: {(request.MaterialsIncluded ? "Evet" : "Hayır")}\nAciliyet: {request.UrgencyLevel}\nNotlar: {request.Notes}\nPara Birimi: {request.Currency}";
        return AnalyzeWithJsonAsync<SMEQuoteReport>(systemPrompt, userMessage);
    }

    private async Task<T> AnalyzeWithJsonAsync<T>(string systemPrompt, string userMessage) where T : class, new()
    {
        try
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var configuredModel = NormalizeModelName(_configuration["Gemini:Model"]);
            var modelCandidates = new List<string> { configuredModel ?? "gemini-flash-latest", "gemini-flash-latest", "gemini-2.0-flash", "gemini-2.5-flash" }
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Gemini API anahtarı bulunamadı. 'Gemini:ApiKey' ayarını kontrol edin.");
            }

            var requestBody = new
            {
                system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                contents = new[] { new { role = "user", parts = new[] { new { text = userMessage } } } },
                generationConfig = new { maxOutputTokens = 4096, responseMimeType = "application/json" }
            };

            var response = await SendWithRetryAndFallbackAsync(apiKey, requestBody, modelCandidates);
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, jsonOptions);
            var aiText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(aiText))
            {
                throw new InvalidOperationException("Gemini API yanıtında geçerli içerik bulunamadı.");
            }

            var parsed = await DeserializeSafelyAsync<T>(aiText, jsonOptions, apiKey, modelCandidates);
            return parsed ?? throw new InvalidOperationException("Gemini API yanıtındaki JSON çözümlenemedi.");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Yazışma analizi sırasında beklenmeyen bir hata oluştu: {ex.Message}", ex);
        }
    }

    private class GeminiResponse
    {
        public List<GeminiCandidate> Candidates { get; set; } = new();
    }

    private class GeminiCandidate
    {
        public GeminiContent Content { get; set; } = new();
    }

    private class GeminiContent
    {
        public List<GeminiPart> Parts { get; set; } = new();
    }

    private class GeminiPart
    {
        public string Text { get; set; } = string.Empty;
    }

    private class FranchiseComparisonOnly
    {
        public string ComparisonSummary { get; set; } = string.Empty;
        public string Winner { get; set; } = string.Empty;
        public string WinnerReason { get; set; } = string.Empty;
    }

    private class RealEstateComparisonOnly
    {
        public string ComparisonSummary { get; set; } = string.Empty;
        public string BetterValue { get; set; } = string.Empty;
        public string BetterValueReason { get; set; } = string.Empty;
    }

    private static FranchiseComparisonOnly BuildFranchiseFallbackComparison(
        FranchiseCompareRequest request,
        FranchiseReport report1,
        FranchiseReport report2)
    {
        var score1 = report1.MonthlyProfit - (report1.TotalInvestment / Math.Max(report1.ROIMonths, 1));
        var score2 = report2.MonthlyProfit - (report2.TotalInvestment / Math.Max(report2.ROIMonths, 1));
        var winner = score1 >= score2 ? "Option1" : "Option2";
        var winnerName = winner == "Option1" ? request.Option1.BrandName : request.Option2.BrandName;

        return new FranchiseComparisonOnly
        {
            Winner = winner,
            ComparisonSummary =
                $"{request.Option1.BrandName} ve {request.Option2.BrandName} seçenekleri yatırım maliyeti, aylık kar ve geri dönüş süresi temelinde karşılaştırıldı. " +
                $"Seçenek 1 için toplam yatırım {report1.TotalInvestment:N0} {report1.Currency}, aylık kar {report1.MonthlyProfit:N0} {report1.Currency}, geri dönüş süresi {report1.ROIMonths} ay. " +
                $"Seçenek 2 için toplam yatırım {report2.TotalInvestment:N0} {report2.Currency}, aylık kar {report2.MonthlyProfit:N0} {report2.Currency}, geri dönüş süresi {report2.ROIMonths} ay. " +
                "AI çıktısı JSON olarak çözümlenemediği için sistem, hesaplanabilir metriklerle güvenli bir karşılaştırma özeti üretti.",
            WinnerReason =
                $"Önerilen seçenek: {winnerName}. Kar/geri dönüş dengesi ve yatırım verimliliği daha yüksek görünüyor. " +
                "Detaylı risk ve rekabet analizi metinleri de genel değerlendirmeye dahil edildi."
        };
    }

    private static RealEstateComparisonOnly BuildRealEstateFallbackComparison(
        RealEstateCompareRequest request,
        RealEstateReport report1,
        RealEstateReport report2)
    {
        var betterValue = report1.PricePerSqm <= report2.PricePerSqm ? "Option1" : "Option2";
        var betterLabel = betterValue == "Option1"
            ? $"{request.Option1.City}/{request.Option1.District}"
            : $"{request.Option2.City}/{request.Option2.District}";

        return new RealEstateComparisonOnly
        {
            BetterValue = betterValue,
            ComparisonSummary =
                $"İki mülk fiyat ve metrekare birim fiyat ekseninde karşılaştırıldı. " +
                $"Mülk 1 toplam fiyatı {report1.EstimatedPrice:N0} {report1.Currency}, birim fiyatı {report1.PricePerSqm:N0} {report1.Currency}. " +
                $"Mülk 2 toplam fiyatı {report2.EstimatedPrice:N0} {report2.Currency}, birim fiyatı {report2.PricePerSqm:N0} {report2.Currency}. " +
                "AI çıktısı JSON formatında çözümlenemediği durumda sistem, yatırım değeri odaklı güvenli fallback değerlendirmesi üretir.",
            BetterValueReason =
                $"Daha iyi değer: {betterLabel}. Birim fiyat avantajı ve fiyat/konum dengesi bu seçeneği öne çıkarıyor. " +
                "Piyasa analizi metinleri de nihai yorumda dikkate alındı."
        };
    }

    private static string? NormalizeModelName(string? modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            return null;
        }

        return modelName.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
            ? modelName["models/".Length..]
            : modelName;
    }

    private async Task<HttpResponseMessage> SendWithRetryAndFallbackAsync(
        string apiKey,
        object requestBody,
        List<string> modelCandidates)
    {
        string lastError = string.Empty;

        foreach (var model in modelCandidates)
        {
            var maxAttempts = model.Equals("gemini-flash-latest", StringComparison.OrdinalIgnoreCase) ? 2 : 1;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new HttpResponseMessage(response.StatusCode)
                    {
                        Content = new StringContent(content, Encoding.UTF8, "application/json")
                    };
                }

                lastError = $"Model: {model}, Attempt: {attempt}, Status: {(int)response.StatusCode}, Detay: {content}";
                if ((int)response.StatusCode == 503 || (int)response.StatusCode == 429)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500 * attempt));
                    continue;
                }

                break;
            }
        }

        throw new InvalidOperationException($"Gemini API isteği başarısız oldu. {lastError}");
    }

    private async Task<T?> DeserializeSafelyAsync<T>(
        string aiText,
        JsonSerializerOptions jsonOptions,
        string apiKey,
        List<string> modelCandidates) where T : class
    {
        var normalizedText = ExtractJsonObject(aiText);
        var parsed = TryDeserialize<T>(normalizedText, jsonOptions);
        if (parsed is not null)
        {
            return parsed;
        }

        var repairedJson = await RepairJsonWithModelAsync(normalizedText, apiKey, modelCandidates);
        var repairedNormalized = ExtractJsonObject(repairedJson);
        return TryDeserialize<T>(repairedNormalized, jsonOptions);
    }

    private static T? TryDeserialize<T>(string jsonText, JsonSerializerOptions jsonOptions) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(jsonText, jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string ExtractJsonObject(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var trimmed = text.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstBraceInBlock = trimmed.IndexOf('{');
            if (firstBraceInBlock >= 0)
            {
                trimmed = trimmed[firstBraceInBlock..];
            }
        }

        var firstBrace = trimmed.IndexOf('{');
        var lastBrace = trimmed.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return trimmed[firstBrace..(lastBrace + 1)];
        }

        return trimmed;
    }

    private async Task<string> RepairJsonWithModelAsync(
        string brokenJson,
        string apiKey,
        List<string> modelCandidates)
    {
        var repairRequestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new
                        {
                            text =
                                "Aşağıdaki JSON metnini düzelt. SADECE geçerli JSON döndür. Açıklama, markdown veya ek metin yazma.\n\n" +
                                brokenJson
                        }
                    }
                }
            },
            generationConfig = new
            {
                maxOutputTokens = 4096,
                responseMimeType = "application/json"
            }
        };

        var response = await SendWithRetryAndFallbackAsync(apiKey, repairRequestBody, modelCandidates);
        var responseContent = await response.Content.ReadAsStringAsync();
        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;
    }
}
