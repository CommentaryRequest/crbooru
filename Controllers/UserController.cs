using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;

namespace CRbooru.Controllers;

[Route("users")]
public class UserController : Controller
{
    private readonly UserService _service;

    public UserController(UserService service)
    {
        _service = service;
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
}
