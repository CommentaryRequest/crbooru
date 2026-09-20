using Microsoft.AspNetCore.Mvc;

namespace CRbooru.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Post");
    }
}
