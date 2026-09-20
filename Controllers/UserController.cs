using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using CRbooru.Services;
using CRbooru.Models;

namespace CRbooru.Controllers;

[Route("users")]
public class UserController : Controller
{
    private readonly UserService _service;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserController(UserService service, IPasswordHasher<User> passwordHasher)
    {
        _service = service;
        _passwordHasher = passwordHasher;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Show(int id)
    {
        var user = await _service.Get(id);
        if (user == null) {
            return NotFound();
        }

        return View(user);
    }

    [HttpGet("login")]
    public IActionResult ShowLogin()
    {
        if (User.Identity?.IsAuthenticated == true) {
            return RedirectToAction("Index", "Post");
        }

        return View("Login");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (User.Identity?.IsAuthenticated == true) {
            return RedirectToAction("Index", "Post");
        }

        // Does the user exist?
        var user = await _service.ByName(model.Username);
        if (user == null) {
            Response.StatusCode = 400;
            ViewData["Error"] = "User does not exist";
            return View();
        }

        // Is the password correct?
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            model.Password);
        if (result == PasswordVerificationResult.Failed) {
            Response.StatusCode = 401;
            ViewData["Error"] = "Incorrect password";
            return View();
        }

        // Create an authenticated identity
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name)
        };

        var identity = new ClaimsIdentity(claims, "CRbooruSession");
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync("CRbooruSession", principal);

        ViewData["Information"] = "You have been logged in";
        return RedirectToAction("Index", "Post");
    }
}
