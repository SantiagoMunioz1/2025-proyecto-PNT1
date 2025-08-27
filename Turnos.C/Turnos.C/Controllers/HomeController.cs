using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Turnos.C.Data;
using Turnos.C.Models;

namespace Turnos.C.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<Persona> _userManager;
    private readonly SignInManager<Persona> _signInManager;
    private readonly TurnosContext _context;
    public HomeController(
        TurnosContext context,
        ILogger<HomeController> logger,
        UserManager<Persona> userManager,
        SignInManager<Persona> signInManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> Index(string mensaje)
    {
        if (_signInManager.IsSignedIn(User))
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.Nombre = user?.Nombre ?? user?.UserName;
        }
        try
        {
            ViewBag.DbVacia = !_userManager.Users.Any();
        }
        catch
        {
            ViewBag.DbVacia = true; // Si falla la conexión o la db aun no tiene ningun user
        }
        ViewBag.Mensaje = mensaje;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}