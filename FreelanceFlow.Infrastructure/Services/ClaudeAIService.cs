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
        try
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var configuredModel = NormalizeModelName(_configuration["Gemini:Model"]);
            var modelCandidates = new List<string>
            {
                configuredModel ?? "gemini-2.5-flash",
                "gemini-flash-latest",
                "gemini-2.0-flash"
            }.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Gemini API anahtarı bulunamadı. 'Gemini:ApiKey' ayarını kontrol edin.");
            }

            const string systemPrompt = "Sen profesyonel bir freelancer asistanısın. Sana verilen müşteri-freelancer yazışmasını analiz edeceksin. Yazışmadan proje kapsamını, fiyatı, süreyi, ödeme koşullarını çıkar. Yanıtını SADECE aşağıdaki JSON formatında ver, başka hiçbir şey yazma, markdown işareti kullanma, sadece düz JSON yaz:\n{\n  \"proposal\": {\n    \"projectTitle\": \"Proje başlığı\",\n    \"scope\": \"Proje kapsamı detaylı açıklama\",\n    \"deliverables\": [\"Teslim edilecek 1\", \"Teslim edilecek 2\"],\n    \"timeline\": \"Süre bilgisi\",\n    \"price\": 0,\n    \"currency\": \"TL\",\n    \"paymentTerms\": \"Ödeme koşulları\"\n  },\n  \"contract\": {\n    \"freelancerName\": \"Freelancer adı\",\n    \"clientName\": \"Müşteri adı\",\n    \"projectTitle\": \"Proje başlığı\",\n    \"startDate\": \"Başlangıç tarihi\",\n    \"endDate\": \"Bitiş tarihi\",\n    \"paymentTerms\": \"Ödeme koşulları detaylı\",\n    \"totalPrice\": 0,\n    \"currency\": \"TL\",\n    \"clauses\": [\n      \"Taraflar işbu sözleşme kapsamında belirtilen yükümlülüklerini eksiksiz yerine getirecektir.\",\n      \"Proje tesliminde gecikme olması halinde müşteri yazılı bildirimde bulunacaktır.\",\n      \"Ödeme belirtilen vadelerde banka havalesi yoluyla yapılacaktır.\",\n      \"Fikri mülkiyet hakları ödemenin tamamlanmasıyla müşteriye devredilecektir.\",\n      \"Taraflardan biri sözleşmeyi feshetmek isterse 15 gün önceden yazılı bildirimde bulunmalıdır.\"\n    ]\n  },\n  \"invoice\": {\n    \"invoiceNo\": \"FRF-2026-001\",\n    \"freelancerName\": \"Freelancer adı\",\n    \"clientName\": \"Müşteri adı\",\n    \"items\": [{\"description\": \"Hizmet açıklaması\", \"quantity\": 1, \"unitPrice\": 0, \"total\": 0}],\n    \"subtotal\": 0,\n    \"kdvRate\": 0.20,\n    \"kdvAmount\": 0,\n    \"total\": 0,\n    \"currency\": \"TL\",\n    \"bankName\": \"Örnek Banka\",\n    \"iban\": \"TR00 0000 0000 0000 0000 0000 00\",\n    \"dueDate\": \"Ödeme tarihi\"\n  },\n  \"followUpEmail\": \"Müşteriye gönderilecek profesyonel takip e-postası metni\"\n}\nEğer yazışmada fiyat belirtilmemişse sektör standardına göre makul bir fiyat öner. Tüm metinler Türkçe olmalı. Fiyatları number olarak yaz.";

            var userMessage =
                $"Freelancer: {freelancerName}\nMüşteri: {clientName}\nSektör: {sector}\nPara Birimi: {currency}\n\nYazışma:\n{rawText}";

            var requestBody = new
            {
                system_instruction = new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = systemPrompt
                        }
                    }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new
                            {
                                text = userMessage
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

            string responseContent = string.Empty;
            var response = await SendWithRetryAndFallbackAsync(apiKey, requestBody, modelCandidates);
            responseContent = await response.Content.ReadAsStringAsync();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, jsonOptions);
            var aiText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(aiText))
            {
                throw new InvalidOperationException("Gemini API yanıtında geçerli içerik bulunamadı.");
            }

            var parsed = JsonSerializer.Deserialize<GeneratedDocuments>(aiText, jsonOptions);
            if (parsed is null)
            {
                throw new InvalidOperationException("Gemini API yanıtındaki JSON çözümlenemedi.");
            }

            return parsed;
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
            for (var attempt = 1; attempt <= 3; attempt++)
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
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
                    continue;
                }

                break;
            }
        }

        throw new InvalidOperationException($"Gemini API isteği başarısız oldu. {lastError}");
    }
}
