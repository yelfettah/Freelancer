using System.Security.Claims;
using System.Text.Json;
using FreelanceFlow.Core.Entities;
using FreelanceFlow.Core.Interfaces;
using FreelanceFlow.Core.Models;
using Microsoft.AspNetCore.Mvc;
using FreelanceFlow.Web.Extensions;
using FreelanceFlow.Web.Models;

namespace FreelanceFlow.Web.Controllers;

public class HistoryController : Controller
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IPdfGeneratorService _pdfGeneratorService;

    public HistoryController(IConversationRepository conversationRepository, IPdfGeneratorService pdfGeneratorService)
    {
        _conversationRepository = conversationRepository;
        _pdfGeneratorService = pdfGeneratorService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return View(BuildSessionConversations());
            }

            var conversations = await _conversationRepository.GetByUserIdAsync(userId);
            var sessionConversations = BuildSessionConversations();
            var merged = conversations.Concat(sessionConversations)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
            return View(merged);
        }
        catch
        {
            return View(BuildSessionConversations());
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var history = HttpContext.Session.GetObject<List<HistoryItem>>("HistoryItems") ?? new List<HistoryItem>();
        if (id < 0)
        {
            var sessionIndex = Math.Abs(id) - 1;
            if (sessionIndex >= 0 && sessionIndex < history.Count)
            {
                history.RemoveAt(sessionIndex);
                HttpContext.Session.SetObject("HistoryItems", history);
            }
        }
        else if (id > 0)
        {
            try { await _conversationRepository.DeleteAsync(id); } catch { }
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id, DocumentType docType)
    {
        if (id < 0)
        {
            var sessionIndex = Math.Abs(id) - 1;
            var history = HttpContext.Session.GetObject<List<HistoryItem>>("HistoryItems") ?? new List<HistoryItem>();
            if (sessionIndex < 0 || sessionIndex >= history.Count)
            {
                return RedirectToAction("Index");
            }

            var item = history[sessionIndex];
            if (string.IsNullOrWhiteSpace(item.GeneratedDocumentsJson))
            {
                return RedirectToAction("Index");
            }

            var generated = JsonSerializer.Deserialize<GeneratedDocuments>(item.GeneratedDocumentsJson);
            if (generated is null)
            {
                return RedirectToAction("Index");
            }

            var sessionFileName = docType switch
            {
                DocumentType.Proposal => "Teklif.pdf",
                DocumentType.Contract => "Sozlesme.pdf",
                DocumentType.Invoice => "Fatura.pdf",
                _ => "Belge.pdf"
            };

            var pdfBytes = docType switch
            {
                DocumentType.Proposal => _pdfGeneratorService.GenerateProposalPdf(generated.Proposal),
                DocumentType.Contract => _pdfGeneratorService.GenerateContractPdf(generated.Contract),
                DocumentType.Invoice => _pdfGeneratorService.GenerateInvoicePdf(generated.Invoice),
                _ => Array.Empty<byte>()
            };

            if (pdfBytes.Length == 0)
            {
                return RedirectToAction("Index");
            }

            return File(pdfBytes, "application/pdf", sessionFileName);
        }

        if (id == 0)
        {
            return RedirectToAction("Index");
        }

        Conversation conversation;
        try
        {
            conversation = await _conversationRepository.GetByIdAsync(id);
        }
        catch
        {
            return RedirectToAction("Index");
        }

        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdValue, out var userId) && conversation.UserId != userId)
        {
            return Forbid();
        }

        var document = conversation.Documents
            .Where(x => x.DocType == docType && x.PdfData.Length > 0)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (document is null)
        {
            return RedirectToAction("Index");
        }

        var dbFileName = docType switch
        {
            DocumentType.Proposal => "Teklif.pdf",
            DocumentType.Contract => "Sozlesme.pdf",
            DocumentType.Invoice => "Fatura.pdf",
            _ => "Belge.pdf"
        };

        return File(document.PdfData, "application/pdf", dbFileName);
    }

    private List<Conversation> BuildSessionConversations()
    {
        var items = HttpContext.Session.GetObject<List<HistoryItem>>("HistoryItems") ?? new List<HistoryItem>();
        return items.Select((item, index) => new Conversation
        {
            Id = -1 - index,
            ClientName = item.Title,
            Sector = item.Module,
            CreatedAt = item.CreatedAt,
            Documents = string.IsNullOrWhiteSpace(item.GeneratedDocumentsJson)
                ? new List<GeneratedDocument>()
                : new List<GeneratedDocument>
                {
                    new() { DocType = DocumentType.Proposal },
                    new() { DocType = DocumentType.Contract },
                    new() { DocType = DocumentType.Invoice }
                }
        }).ToList();
    }
}
