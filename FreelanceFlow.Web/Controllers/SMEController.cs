using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Core.Models;
using Microsoft.AspNetCore.Mvc;
using FreelanceFlow.Web.Extensions;
using FreelanceFlow.Web.Models;

namespace FreelanceFlow.Web.Controllers;

public class SMEController : Controller
{
    private readonly IClaudeAIService _aiService;
    private readonly IPdfGeneratorService _pdfGeneratorService;

    public SMEController(IClaudeAIService aiService, IPdfGeneratorService pdfGeneratorService)
    {
        _aiService = aiService;
        _pdfGeneratorService = pdfGeneratorService;
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Generate(SMEQuoteRequest model)
    {
        var result = await _aiService.GenerateSMEQuoteAsync(model);
        TempData["SMERequest"] = System.Text.Json.JsonSerializer.Serialize(model);
        TempData["SMEResult"] = System.Text.Json.JsonSerializer.Serialize(result);
        AddHistory($"{model.BusinessType} - {model.ClientName}");
        return RedirectToAction("Result");
    }

    [HttpGet]
    public IActionResult Result()
    {
        var request = TempData.Peek("SMERequest") as string;
        var result = TempData.Peek("SMEResult") as string;
        if (string.IsNullOrWhiteSpace(request) || string.IsNullOrWhiteSpace(result))
            return RedirectToAction("Create");
        ViewBag.Request = System.Text.Json.JsonSerializer.Deserialize<SMEQuoteRequest>(request);
        return View(System.Text.Json.JsonSerializer.Deserialize<SMEQuoteReport>(result));
    }

    [HttpGet]
    public IActionResult DownloadReport()
    {
        var request = TempData.Peek("SMERequest") as string;
        var result = TempData.Peek("SMEResult") as string;
        if (string.IsNullOrWhiteSpace(request) || string.IsNullOrWhiteSpace(result))
            return RedirectToAction("Create");
        var requestData = System.Text.Json.JsonSerializer.Deserialize<SMEQuoteRequest>(request) ?? new SMEQuoteRequest();
        var resultData = System.Text.Json.JsonSerializer.Deserialize<SMEQuoteReport>(result) ?? new SMEQuoteReport();
        var pdf = _pdfGeneratorService.GenerateSMEQuotePdf(resultData, requestData);
        return File(pdf, "application/pdf", "HizmetTeklifi.pdf");
    }

    private void AddHistory(string title)
    {
        var history = HttpContext.Session.GetObject<List<HistoryItem>>("HistoryItems") ?? new List<HistoryItem>();
        history.Insert(0, new HistoryItem { Module = "SME", Title = title, CreatedAt = DateTime.UtcNow });
        HttpContext.Session.SetObject("HistoryItems", history);
    }
}
