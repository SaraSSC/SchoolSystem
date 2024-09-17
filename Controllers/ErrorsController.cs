using Microsoft.AspNetCore.Mvc;
using SchoolSystem.Models;
using System.Diagnostics;

namespace SchoolSystem.Controllers
{
    public class ErrorsController : Controller
    {
        // Handles the default error page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Create a new ErrorViewModel and set the RequestId property to the current Activity Id or HttpContext TraceIdentifier
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Handles the custom 404 error page
        [Route("error/404")]
        public IActionResult Error404()
        {
            return View();
        }
    }
}
