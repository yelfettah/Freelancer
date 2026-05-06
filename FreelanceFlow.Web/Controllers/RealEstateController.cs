using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Core.Models;
using Microsoft.AspNetCore.Mvc;
using FreelanceFlow.Web.Extensions;
using FreelanceFlow.Web.Models;

namespace FreelanceFlow.Web.Controllers;

public class RealEstateController : Controller
{
    private readonly IClaudeAIService _aiService;
    private readonly IPdfGeneratorService _pdfGeneratorService;

    public RealEstateController(IClaudeAIService aiService, IPdfGeneratorService pdfGeneratorService)
    {
        _aiService = aiService;
        _pdfGeneratorService = pdfGeneratorService;
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpGet]
    public IActionResult Compare() => View();

    [HttpPost]
    public async Task<IActionResult> Generate(RealEstateRequest model)
    {
        var result = await _aiService.AnalyzeRealEstateAsync(model);
        TempData["RealEstateRequest"] = System.Text.Json.JsonSerializer.Serialize(model);
        TempData["RealEstateResult"] = System.Text.Json.JsonSerializer.Serialize(result);
        AddHistory($"{model.PropertyType} - {model.City}/{model.District}");
        return RedirectToAction("Result");
    }

    [HttpGet]
    public IActionResult Result()
    {
        var request = TempData.Peek("RealEstateRequest") as string;
        var result = TempData.Peek("RealEstateResult") as string;
        if (string.IsNullOrWhiteSpace(request) || string.IsNullOrWhiteSpace(result))
            return RedirectToAction("Create");
        ViewBag.Request = System.Text.Json.JsonSerializer.Deserialize<RealEstateRequest>(request);
        return View(System.Text.Json.JsonSerializer.Deserialize<RealEstateReport>(result));
    }

    [HttpGet]
    public IActionResult DownloadReport()
    {
        var request = TempData.Peek("RealEstateRequest") as string;
        var result = TempData.Peek("RealEstateResult") as string;
        if (string.IsNullOrWhiteSpace(request) || string.IsNullOrWhiteSpace(result))
            return RedirectToAction("Create");
        var requestData = System.Text.Json.JsonSerializer.Deserialize<RealEstateRequest>(request) ?? new RealEstateRequest();
        var resultData = System.Text.Json.JsonSerializer.Deserialize<RealEstateReport>(result) ?? new RealEstateReport();
        var pdf = _pdfGeneratorService.GenerateRealEstateReportPdf(resultData, requestData);
        return File(pdf, "application/pdf", "EmlakRaporu.pdf");
    }

    [HttpPost]
    public async Task<IActionResult> CompareGenerate(RealEstateCompareRequest model)
    {
        try
        {
            var result = await _aiService.CompareRealEstateAsync(model);
            TempData["RealEstateCompareResult"] = System.Text.Json.JsonSerializer.Serialize(result);
            AddHistory($"Emlak Karşılaştırma - {model.Option1.City}/{model.Option1.District} vs {model.Option2.City}/{model.Option2.District}");
            return RedirectToAction("CompareResult");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Karşılaştırma sırasında hata oluştu: {ex.Message}");
            return View("Compare", model);
        }
    }

    [HttpGet]
    public IActionResult CompareResult()
    {
        var resultJson = TempData.Peek("RealEstateCompareResult") as string;
        if (string.IsNullOrWhiteSpace(resultJson))
            return RedirectToAction("Compare");
        var result = System.Text.Json.JsonSerializer.Deserialize<RealEstateCompareResult>(resultJson);
        return View(result);
    }

    [HttpGet]
    public IActionResult DownloadComparison()
    {
        var resultJson = TempData.Peek("RealEstateCompareResult") as string;
        if (string.IsNullOrWhiteSpace(resultJson))
            return RedirectToAction("Compare");
        var result = System.Text.Json.JsonSerializer.Deserialize<RealEstateCompareResult>(resultJson) ?? new RealEstateCompareResult();
        var pdf = _pdfGeneratorService.GenerateRealEstateComparisonPdf(result);
        return File(pdf, "application/pdf", "EmlakKarsilastirma.pdf");
    }

    private void AddHistory(string title)
    {
        var history = HttpContext.Session.GetObject<List<HistoryItem>>("HistoryItems") ?? new List<HistoryItem>();
        history.Insert(0, new HistoryItem { Module = "RealEstate", Title = title, CreatedAt = DateTime.UtcNow });
        HttpContext.Session.SetObject("HistoryItems", history);
    }
}
