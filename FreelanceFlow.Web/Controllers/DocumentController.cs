using System.IO.Compression;
using System.Text.Json;
using FreelanceFlow.Core.Entities;
using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Core.Models;
using Microsoft.AspNetCore.Mvc;
using FreelanceFlow.Web.Extensions;
using FreelanceFlow.Web.Models;

namespace FreelanceFlow.Web.Controllers;

public class DocumentController : Controller
{
    private readonly IClaudeAIService _claudeAiService;
    private readonly IPdfGeneratorService _pdfGeneratorService;

    public DocumentController(
        IClaudeAIService claudeAiService,
        IPdfGeneratorService pdfGeneratorService)
    {
        _claudeAiService = claudeAiService;
        _pdfGeneratorService = pdfGeneratorService;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Generate(ConversationInput model)
    {
        if (!ModelState.IsValid)
        {
            return View("Create", model);
        }

        try
        {
            var profile = HttpContext.Session.GetObject<FreelancerProfile>("FreelancerProfile");
            var parameters = ReadCustomParametersFromRequest();
            var result = profile is null
                ? await _claudeAiService.AnalyzeConversationAsync(model.RawText, model.FreelancerName, model.ClientName, model.Sector, model.Currency)
                : await _claudeAiService.AnalyzeConversationWithProfileAsync(
                    model.RawText,
                    model.FreelancerName,
                    model.ClientName,
                    model.Sector,
                    model.Currency,
                    profile,
                    parameters);

            TempData["GeneratedResult"] = JsonSerializer.Serialize(result);
            TempData["ConversationInput"] = JsonSerializer.Serialize(model);
            AddHistoryItem("Freelancer", $"{model.ClientName} - {model.Sector}", result);

            return RedirectToAction("Result");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Belge üretimi sırasında hata oluştu: {ex.Message}");
            return View("Create", model);
        }
    }

    [HttpGet]
    public IActionResult Result()
    {
        var generatedResultJson = TempData["GeneratedResult"] as string;
        var conversationInputJson = TempData["ConversationInput"] as string;

        if (string.IsNullOrWhiteSpace(generatedResultJson) || string.IsNullOrWhiteSpace(conversationInputJson))
        {
            return RedirectToAction("Create");
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var generatedResult = JsonSerializer.Deserialize<GeneratedDocuments>(generatedResultJson, options);
        var conversationInput = JsonSerializer.Deserialize<ConversationInput>(conversationInputJson, options);

        if (generatedResult is null || conversationInput is null)
        {
            return RedirectToAction("Create");
        }

        ViewBag.GeneratedResult = generatedResult;
        ViewBag.ConversationInput = conversationInput;

        TempData["GeneratedResult"] = generatedResultJson;
        TempData["ConversationInput"] = conversationInputJson;

        return View(generatedResult);
    }

    [HttpGet]
    public IActionResult DownloadProposal()
    {
        var generated = GetGeneratedResultFromTempData();
        if (generated is null)
        {
            return RedirectToAction("Create");
        }

        var pdfBytes = _pdfGeneratorService.GenerateProposalPdf(generated.Proposal);
        return File(pdfBytes, "application/pdf", "Teklif.pdf");
    }

    [HttpGet]
    public IActionResult DownloadContract()
    {
        var generated = GetGeneratedResultFromTempData();
        if (generated is null)
        {
            return RedirectToAction("Create");
        }

        var pdfBytes = _pdfGeneratorService.GenerateContractPdf(generated.Contract);
        return File(pdfBytes, "application/pdf", "Sozlesme.pdf");
    }

    [HttpGet]
    public IActionResult DownloadInvoice()
    {
        var generated = GetGeneratedResultFromTempData();
        if (generated is null)
        {
            return RedirectToAction("Create");
        }

        var pdfBytes = _pdfGeneratorService.GenerateInvoicePdf(generated.Invoice);
        return File(pdfBytes, "application/pdf", "Fatura.pdf");
    }

    [HttpGet]
    public IActionResult DownloadAll()
    {
        var generated = GetGeneratedResultFromTempData();
        if (generated is null)
        {
            return RedirectToAction("Create");
        }

        var proposalPdf = _pdfGeneratorService.GenerateProposalPdf(generated.Proposal);
        var contractPdf = _pdfGeneratorService.GenerateContractPdf(generated.Contract);
        var invoicePdf = _pdfGeneratorService.GenerateInvoicePdf(generated.Invoice);

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            AddFileToZip(archive, "Teklif.pdf", proposalPdf);
            AddFileToZip(archive, "Sozlesme.pdf", contractPdf);
            AddFileToZip(archive, "Fatura.pdf", invoicePdf);
        }

        return File(memoryStream.ToArray(), "application/zip", "FreelanceFlow_Belgeler.zip");
    }

    private GeneratedDocuments? GetGeneratedResultFromTempData()
    {
        var generatedResultJson = TempData.Peek("GeneratedResult") as string;
        if (string.IsNullOrWhiteSpace(generatedResultJson))
        {
            return null;
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<GeneratedDocuments>(generatedResultJson, options);
    }

    private static void AddFileToZip(ZipArchive archive, string fileName, byte[] content)
    {
        var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        entryStream.Write(content, 0, content.Length);
    }

    private List<CustomParameter> ReadCustomParametersFromRequest()
    {
        var list = new List<CustomParameter>();
        var indexedNames = Request.Form.Keys
            .Where(key => key.StartsWith("Parameters[", StringComparison.OrdinalIgnoreCase) && key.EndsWith("].Name", StringComparison.OrdinalIgnoreCase))
            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (indexedNames.Count > 0)
        {
            for (var i = 0; i < indexedNames.Count; i++)
            {
                var nameKey = indexedNames[i];
                var prefix = nameKey[..^".Name".Length];
                var valueKey = $"{prefix}.Value";
                var descriptionKey = $"{prefix}.Description";
                var name = Request.Form[nameKey].ToString();
                var value = Request.Form[valueKey].ToString();
                var description = Request.Form[descriptionKey].ToString();
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                list.Add(new CustomParameter
                {
                    Id = i + 1,
                    Name = name,
                    Value = value,
                    Description = description
                });
            }
        }
        else
        {
            var names = Request.Form["CustomParamName"];
            var values = Request.Form["CustomParamValue"];
            var descriptions = Request.Form["CustomParamDescription"];
            for (var i = 0; i < names.Count; i++)
            {
                var name = names.ElementAtOrDefault(i)?.ToString() ?? string.Empty;
                var value = values.ElementAtOrDefault(i)?.ToString() ?? string.Empty;
                var description = descriptions.ElementAtOrDefault(i)?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                list.Add(new CustomParameter
                {
                    Id = i + 1,
                    Name = name,
                    Value = value,
                    Description = description
                });
            }
        }

        HttpContext.Session.SetObject("CustomParameters", list);
        return list;
    }

    private void AddHistoryItem(string module, string title, GeneratedDocuments generatedDocuments)
    {
        var history = HttpContext.Session.GetObject<List<HistoryItem>>("HistoryItems") ?? new List<HistoryItem>();
        history.Insert(0, new HistoryItem
        {
            Module = module,
            Title = title,
            CreatedAt = DateTime.UtcNow,
            GeneratedDocumentsJson = JsonSerializer.Serialize(generatedDocuments)
        });
        HttpContext.Session.SetObject("HistoryItems", history.Take(100).ToList());
    }
}
