using Microsoft.AspNetCore.Mvc;
using ProyectoU1.Models;
using System.Diagnostics;

namespace ProyectoU1.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Login", "Account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
