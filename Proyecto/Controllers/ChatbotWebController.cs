using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Proyecto.Autenticacion;

namespace Proyecto.Controllers
{
    [Authorize(Policy = "CuentaUsuario")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ChatbotWebController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
