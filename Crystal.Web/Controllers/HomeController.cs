using Microsoft.AspNetCore.Mvc;

namespace Crystal.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction("Login", "Account");
        }

        return RedirectToAction("Index", "Tasks");
    }

    public IActionResult Error()
    {
        return View();
    }
}
