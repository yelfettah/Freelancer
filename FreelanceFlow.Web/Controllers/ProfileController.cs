using FreelanceFlow.Core.Models;
using Microsoft.AspNetCore.Mvc;
using FreelanceFlow.Web.Extensions;

namespace FreelanceFlow.Web.Controllers;

public class ProfileController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var profile = HttpContext.Session.GetObject<FreelancerProfile>("FreelancerProfile") ?? new FreelancerProfile();
        return View(profile);
    }

    [HttpPost]
    public IActionResult Save(FreelancerProfile model)
    {
        model.CreatedAt = model.CreatedAt == default ? DateTime.UtcNow : model.CreatedAt;
        HttpContext.Session.SetObject("FreelancerProfile", model);
        TempData["Message"] = "Profil kaydedildi.";
        return RedirectToAction("Index");
    }
}
