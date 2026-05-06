using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Core.Models;
using Microsoft.AspNetCore.Mvc;
using FreelanceFlow.Web.Extensions;
using FreelanceFlow.Web.Models;

namespace FreelanceFlow.Web.Controllers;

public class LawyerController : Controller
{
    private readonly IClaudeAIService _aiService;
    private readonly IPdfGeneratorService _pdfGeneratorService;

    public LawyerController(IClaudeAIService aiService, IPdfGeneratorService pdfGeneratorService)
    {
        _aiService = aiService;
        _pdfGeneratorService = pdfGeneratorService;
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Generate(LawyerRequest model)
    {
        var result = await _aiService.AnalyzeLawyerCaseAsync(model);
        TempData["LawyerRequest"] = System.Text.Json.JsonSerializer.Serialize(model);
        TempData["LawyerResult"] = System.Text.Json.JsonSerializer.Serialize(result);
        AddHistory($"{model.CaseType} - {model.City}");
        return RedirectToAction("Result");
    }

    [HttpGet]
    public IActionResult Result()
    {
        var request = TempData.Peek("LawyerRequest") as string;
        var result = TempData.Peek("LawyerResult") as string;
        if (string.IsNullOrWhiteSpace(request) || string.IsNullOrWhiteSpace(result))
            return RedirectToAction("Create");
        ViewBag.Request = System.Text.Json.JsonSerializer.Deserialize<LawyerRequest>(request);
        return View(System.Text.Json.JsonSerializer.Deserialize<LawyerReport>(result));
    }

    [HttpGet]
    public IActionResult DownloadReport()
    {
        var request = TempData.Peek("LawyerRequest") as string;
        var result = TempData.Peek("LawyerResult") as string;
        if (string.IsNullOrWhiteSpace(request) || string.IsNullOrWhiteSpace(result))
            return RedirectToAction("Create");
        var requestData = System.Text.Json.JsonSerializer.Deserialize<LawyerRequest>(request) ?? new LawyerRequest();
        var resultData = System.Text.Json.JsonSerializer.Deserialize<LawyerReport>(result) ?? new LawyerReport();
        var pdf = _pdfGeneratorService.GenerateLawyerReportPdf(resultData, requestData);
        return File(pdf, "application/pdf", "HukukiRapor.pdf");
    }

    private void AddHistory(string title)
    {
        var history = HttpContext.Session.GetObject<List<HistoryItem>>("HistoryItems") ?? new List<HistoryItem>();
        history.Insert(0, new HistoryItem { Module = "Lawyer", Title = title, CreatedAt = DateTime.UtcNow });
        HttpContext.Session.SetObject("HistoryItems", history);
    }
}
