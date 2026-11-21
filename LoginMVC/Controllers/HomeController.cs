using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoginMVC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "-";
            ViewBag.NombreUsuario = User.FindFirstValue(ClaimTypes.Name) ?? "Usuario";
            ViewBag.CorreoUsuario = User.FindFirstValue(ClaimTypes.Email) ?? "";
            ViewBag.RolUsuario = User.FindFirstValue(ClaimTypes.Role) ?? "Estudiante";
            ViewBag.UltimoAcceso = User.FindFirstValue("UltimoAcceso") ?? "Primer inicio de sesión";
            ViewBag.HoraSesion = User.FindFirstValue("HoraSesion") ?? DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            return View();
        }
    }
}