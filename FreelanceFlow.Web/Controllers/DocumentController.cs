using System.IO.Compression;
using System.Text.Json;
using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Core.Models;
using FreelanceFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceFlow.Web.Controllers;

public class DocumentController : Controller
{
    private readonly IClaudeAIService _claudeAiService;
    private readonly IPdfGeneratorService _pdfGeneratorService;
    private readonly AppDbContext _dbContext;

    public DocumentController(
        IClaudeAIService claudeAiService,
        IPdfGeneratorService pdfGeneratorService,
        AppDbContext dbContext)
    {
        _claudeAiService = claudeAiService;
        _pdfGeneratorService = pdfGeneratorService;
        _dbContext = dbContext;
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
            var result = await _claudeAiService.AnalyzeConversationAsync(
                model.RawText,
                model.FreelancerName,
                model.ClientName,
                model.Sector,
                model.Currency);

            TempData["GeneratedResult"] = JsonSerializer.Serialize(result);
            TempData["ConversationInput"] = JsonSerializer.Serialize(model);

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
}
