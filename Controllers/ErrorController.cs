using Microsoft.AspNetCore.Mvc;

namespace PortalInmobiliario.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}