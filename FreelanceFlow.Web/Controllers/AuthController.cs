using System.Security.Claims;
using FreelanceFlow.Core.Entities;
using FreelanceFlow.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceFlow.Web.Controllers;

public class AuthController : Controller
{
    private readonly IUserRepository _userRepository;

    public AuthController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ViewBag.Error = "E-posta veya şifre hatalı.";
            return View();
        }

        await SignInUserAsync(user);
        return RedirectToAction("Create", "Document");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            ViewBag.Error = "Şifreler eşleşmiyor.";
            return View();
        }

        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser is not null)
        {
            ViewBag.Error = "Bu e-posta zaten kayıtlı.";
            return View();
        }

        var user = new User
        {
            FullName = fullName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        user = await _userRepository.CreateAsync(user);
        await SignInUserAsync(user);

        return RedirectToAction("Create", "Document");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private Task SignInUserAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
