using System.Security.Claims;
using FreelanceFlow.Core.Entities;
using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelanceFlow.Web.Controllers;

[Authorize]
public class HistoryController : Controller
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IPdfGeneratorService _pdfGeneratorService;
    private readonly AppDbContext _dbContext;

    public HistoryController(
        IConversationRepository conversationRepository,
        IPdfGeneratorService pdfGeneratorService,
        AppDbContext dbContext)
    {
        _conversationRepository = conversationRepository;
        _pdfGeneratorService = pdfGeneratorService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var conversations = await _conversationRepository.GetByUserIdAsync(userId);
        return View(conversations);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var isOwner = await _dbContext.Conversations.AnyAsync(c => c.Id == id && c.UserId == userId && !c.IsDeleted);
        if (isOwner)
        {
            await _conversationRepository.DeleteAsync(id);
        }

        _ = _pdfGeneratorService;
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Download(int conversationId, DocumentType type)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var conversation = await _dbContext.Conversations
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId && !c.IsDeleted);

        if (conversation is null)
        {
            return RedirectToAction("Index");
        }

        var document = conversation.Documents
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefault(d => d.DocType == type);

        if (document is null || document.PdfData.Length == 0)
        {
            TempData["HistoryError"] = "Bu kayıt için ilgili PDF bulunamadı.";
            return RedirectToAction("Index");
        }

        var fileName = type switch
        {
            DocumentType.Proposal => "Teklif.pdf",
            DocumentType.Contract => "Sozlesme.pdf",
            DocumentType.Invoice => "Fatura.pdf",
            _ => "Belge.pdf"
        };

        return File(document.PdfData, "application/pdf", fileName);
    }
}
