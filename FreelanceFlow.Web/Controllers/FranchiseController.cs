using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Core.Models;
using Microsoft.AspNetCore.Mvc;
using FreelanceFlow.Web.Extensions;
using FreelanceFlow.Web.Models;

namespace FreelanceFlow.Web.Controllers;

public class FranchiseController : Controller
{
    private readonly IClaudeAIService _aiService;
    private readonly IPdfGeneratorService _pdfGeneratorService;

    public FranchiseController(IClaudeAIService aiService, IPdfGeneratorService pdfGeneratorService)
    {
        _aiService = aiService;
        _pdfGeneratorService = pdfGeneratorService;
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpGet]
    public IActionResult Compare() => View();

    [HttpPost]
    public async Task<IActionResult> Generate(FranchiseRequest model)
    {
        var result = await _aiService.AnalyzeFranchiseAsync(model);
        TempData["FranchiseRequest"] = System.Text.Json.JsonSerializer.Serialize(model);
        TempData["FranchiseResult"] = System.Text.Json.JsonSerializer.Serialize(result);
        AddHistory($"Franchise - {model.BrandName}");
        return RedirectToAction("Result");
    }

    [HttpGet]
    public IActionResult Result()
    {
        var request = TempData.Peek("FranchiseRequest") as string;
        var result = TempData.Peek("FranchiseResult") as string;
        if (string.IsNullOrWhiteSpace(request) || string.IsNullOrWhiteSpace(result))
            return RedirectToAction("Create");

        ViewBag.Request = System.Text.Json.JsonSerializer.Deserialize<FranchiseRequest>(request);
        return View(System.Text.Json.JsonSerializer.Deserialize<FranchiseReport>(result));
    }

    [HttpGet]
    public IActionResult DownloadReport()
    {
        var request = TempData.Peek("FranchiseRequest") as string;
        var result = TempData.Peek("FranchiseResult") as string;
        if (string.IsNullOrWhiteSpace(request) || string.IsNullOrWhiteSpace(result))
            return RedirectToAction("Create");

        var requestData = System.Text.Json.JsonSerializer.Deserialize<FranchiseRequest>(request) ?? new FranchiseRequest();
        var resultData = System.Text.Json.JsonSerializer.Deserialize<FranchiseReport>(result) ?? new FranchiseReport();
        var pdf = _pdfGeneratorService.GenerateFranchiseReportPdf(resultData, requestData);
        return File(pdf, "application/pdf", "FranchiseRaporu.pdf");
    }

    [HttpPost]
    public async Task<IActionResult> CompareGenerate(FranchiseCompareRequest model)
    {
        try
        {
            var result = await _aiService.CompareFranchisesAsync(model);
            TempData["FranchiseCompareResult"] = System.Text.Json.JsonSerializer.Serialize(result);
            AddHistory($"Franchise Karşılaştırma - {model.Option1.BrandName} vs {model.Option2.BrandName}");
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
        var resultJson = TempData.Peek("FranchiseCompareResult") as string;
        if (string.IsNullOrWhiteSpace(resultJson))
            return RedirectToAction("Compare");
        var result = System.Text.Json.JsonSerializer.Deserialize<FranchiseCompareResult>(resultJson);
        return View(result);
    }

    [HttpGet]
    public IActionResult DownloadComparison()
    {
        var resultJson = TempData.Peek("FranchiseCompareResult") as string;
        if (string.IsNullOrWhiteSpace(resultJson))
            return RedirectToAction("Compare");
        var result = System.Text.Json.JsonSerializer.Deserialize<FranchiseCompareResult>(resultJson) ?? new FranchiseCompareResult();
        var pdf = _pdfGeneratorService.GenerateFranchiseComparisonPdf(result);
        return File(pdf, "application/pdf", "FranchiseKarsilastirma.pdf");
    }

    private void AddHistory(string title)
    {
        var history = HttpContext.Session.GetObject<List<HistoryItem>>("HistoryItems") ?? new List<HistoryItem>();
        history.Insert(0, new HistoryItem { Module = "Franchise", Title = title, CreatedAt = DateTime.UtcNow });
        HttpContext.Session.SetObject("HistoryItems", history);
    }
}
