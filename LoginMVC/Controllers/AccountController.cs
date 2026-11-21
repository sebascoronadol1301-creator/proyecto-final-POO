using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LoginMVC.Data;
using LoginMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoginMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string correoNormalizado = model.Correo.Trim().ToLower();
            string passwordHash = EncriptarSha256(model.Password);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == correoNormalizado && u.Password == passwordHash);

            if (usuario == null)
            {
                ViewBag.ErrorMessage = "Correo electrónico o contraseña incorrectos.";
                return View(model);
            }

            string ultimoAccesoAnterior = usuario.UltimoAcceso.HasValue
                ? usuario.UltimoAcceso.Value.ToString("dd/MM/yyyy HH:mm")
                : "Primer inicio de sesión";

            usuario.UltimoAcceso = DateTime.Now;
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("UltimoAcceso", ultimoAccesoAnterior),
                new Claim("HoraSesion", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string correoNormalizado = model.Correo.Trim().ToLower();

            bool existeCorreo = await _context.Usuarios
                .AnyAsync(u => u.Correo.ToLower() == correoNormalizado);

            if (existeCorreo)
            {
                ModelState.AddModelError("Correo", "Este correo electrónico ya está registrado.");
                return View(model);
            }

            var nuevoUsuario = new Usuario
            {
                Nombre = model.Nombre.Trim(),
                Correo = correoNormalizado,
                Password = EncriptarSha256(model.Password),
                Rol = "Estudiante"
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cuenta creada exitosamente. Ya puedes iniciar sesión.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["InfoMessage"] = "Has cerrado sesión correctamente. ¡Hasta pronto!";
            return RedirectToAction("Login", "Account");
        }

        private static string EncriptarSha256(string texto)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}