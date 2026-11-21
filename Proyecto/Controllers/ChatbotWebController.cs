
using Microsoft.AspNetCore.Mvc;

namespace Proyecto.Controllers
{
    public class ChatbotWebController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
