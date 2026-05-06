using System.Security.Claims;
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
}
