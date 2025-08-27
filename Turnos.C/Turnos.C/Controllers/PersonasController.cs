using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Turnos.C.Data;
using Turnos.C.Helpers;
using Turnos.C.Models;
using Turnos.C.Models.ViewModels;

namespace Turnos.C.Controllers
{
    public class PersonasController : Controller
    {
        private readonly TurnosContext _context;
        private readonly UserManager<Persona> _userManager;
        private readonly RoleManager<Rol> _roleManager;

        // Inyección de dependencias necesarias: contexto, manejo de usuarios y de roles
        public PersonasController(
            TurnosContext context,
            UserManager<Persona> userManager,
            RoleManager<Rol> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Personas
        // Muestra el listado de todas las personas registradas
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Personas.ToListAsync());
        }

        // GET: Personas/Details/5
        // Muestra los detalles de una persona específica por su ID
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var persona = await _context.Personas.FirstOrDefaultAsync(m => m.Id == id);
            if (persona == null)
                return NotFound();

            return View(persona);
        }

        // GET: Personas/Create
        // Primer paso para crear una persona: selección del rol
        [Authorize(Roles = Config.AdminRolName)]
        public async Task<IActionResult> Create()
        {
            // Se cargan todos los roles disponibles del sistema para mostrarlos en el formulario
            var roles = await _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                .ToListAsync();

            var vm = new PersonaViewModel
            {
                Roles = roles
            };

            // Se muestra la vista "SelectRole" con el ViewModel cargado
            return View("SelectRole", vm);
        }

        // POST: Personas/Create
        // Procesa la selección del rol y redirige a la acción de creación correspondiente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string SelectedRole)
        {
            // Validación simple para asegurar que se haya seleccionado un rol
            if (string.IsNullOrWhiteSpace(SelectedRole))
            {
                ModelState.AddModelError("SelectedRole", "Debe seleccionar un rol.");
                return View("SelectRole");
            }

            return RedireccionarSegunRol(SelectedRole);
        }

        // Método privado para manejar la lógica de redirección según el rol seleccionado
        private IActionResult RedireccionarSegunRol(string rol)
        {
            // Si el rol es Profesional, se redirige a su controlador correspondiente
            if (rol == "Profesional")
                return RedirectToAction("Create", "Profesionales");

            // Si es Paciente, se redirige al controlador de Pacientes
            if (rol == "Paciente")
                return RedirectToAction("Create", "Pacientes");

            // Para cualquier otro rol (Usuario, Admin, etc.) se usa vista genérica
            return RedirectToAction("CreateGenerico", new { role = rol });
        }

        [Authorize(Roles = Config.AdminRolName)]
        [HttpGet]
        public async Task<IActionResult> EditGenerico(int id)
        {
            var persona = await _context.Personas.FindAsync(id);
            if (persona == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(persona);
            var rol = roles.FirstOrDefault();

            var vm = new GenericUserViewModel
            {
                Id = persona.Id,
                Nombre = persona.Nombre,
                Apellido = persona.Apellido,
                DNI = persona.DNI,
                Email = persona.Email,
                Role = rol
            };

            return View(vm);
        }

        [Authorize(Roles = Config.AdminRolName)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGenerico(GenericUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var persona = await _context.Personas.FindAsync(model.Id);
            if (persona == null)
                return NotFound();

            persona.Nombre = model.Nombre;
            persona.Apellido = model.Apellido;
            persona.Email = model.Email;
            persona.UserName = model.Email;
            persona.DNI = model.DNI;

            try
            {
                _context.Personas.Update(persona);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbex)
            {
                SqlException innerException = dbex.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    ModelState.AddModelError(nameof(model.DNI), "Ya existe una persona registrada con ese DNI.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, dbex.Message);
                }

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }


        // Acción específica para creación de Paciente (opcional, puede estar en desuso si ya se redirige al controller correcto)
        public IActionResult CreatePaciente()
        {
            return View(); // Views/Personas/CreatePaciente.cshtml
        }

        // Acción para crear usuarios genéricos según el rol pasado por parámetro
        [Authorize(Roles = Config.AdminRolName)]
        public IActionResult CreateGenerico(string role)
        {
            var vm = new GenericUserViewModel { Role = role };
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Config.AdminRolName)]
        public async Task<IActionResult> CreateGenerico(GenericUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var persona = new Persona
            {
                Email = model.Email,
                UserName = model.Email,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                DNI = model.DNI,
            };

            try
            {
                var res = await _userManager.CreateAsync(persona, Config.PasswordGenerica);
                if (!res.Succeeded)
                {
                    foreach (var e in res.Errors)
                        ModelState.AddModelError(string.Empty, e.Description);
                    return View(model);
                }

                await _userManager.AddToRoleAsync(persona, model.Role);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbex)
            {
                SqlException innerException = dbex.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    ModelState.AddModelError(nameof(model.DNI), "Ya existe una persona registrada con ese DNI.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, dbex.Message);
                }

                return View(model);
            }
        }



        // GET: Personas/Delete/5
        // Muestra la vista de confirmación para eliminar una persona
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var persona = await _context.Personas.FirstOrDefaultAsync(m => m.Id == id);
            if (persona == null)
                return NotFound();

            return View(persona);
        }

        // POST: Personas/Delete/5
        // Ejecuta la eliminación real de la persona seleccionada
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = $"{Config.AdminRolName}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string returnUrl = null)
        {
            var persona = await _context.Personas.FindAsync(id);
            if (persona == null)
                return NotFound();

            var esPaciente = await _userManager.IsInRoleAsync(persona, Config.PacienteRolName);
            if (esPaciente)
            {
                ModelState.AddModelError(string.Empty, "No está permitido eliminar pacientes del sistema.");
                ViewBag.ReturnUrl = returnUrl;
                return View(persona);
            }

            var tieneTurnos = await _context.Turnos.AnyAsync(t => t.ProfesionalId == id);
            if (tieneTurnos)
            {
                ModelState.AddModelError(string.Empty, "No se puede eliminar el profesional porque tiene turnos asignados.");
                ViewBag.ReturnUrl = returnUrl;
                return View(persona);
            }

            try
            {
                _context.Personas.Remove(persona);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Profesional eliminado exitosamente.";

                // Si returnUrl NO es Details, redirigir ahí
                if (!string.IsNullOrEmpty(returnUrl) &&
                    !returnUrl.Contains("/Details", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(returnUrl);
                }

                // Si returnUrl era a Personas/Details, redirigir SIEMPRE a Personas/Index
                if (!string.IsNullOrEmpty(returnUrl) &&
                    returnUrl.Contains("/Personas/Details", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index", "Personas");
                }

                // Si returnUrl era a otro Details, redirigir según el tipo de persona
                if (persona is Profesional)
                    return RedirectToAction("Index", "Profesionales");

                return RedirectToAction("Index", "Personas");
            }

            catch (DbUpdateException dbex)
            {
                ModelState.AddModelError(string.Empty, "Error al eliminar el profesional: " + dbex.Message);
                ViewBag.ReturnUrl = returnUrl;
                return View(persona);
            }
        }

        [Authorize(Roles = Config.AdminRolName)]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var persona = await _context.Personas.FindAsync(id);

            if (persona == null)
                return NotFound();

            if (await _userManager.IsInRoleAsync(persona, Config.PacienteRolName))
                return RedirectToAction("Edit", "Pacientes", new { id });

            if (await _userManager.IsInRoleAsync(persona, Config.ProfesionalRolName))
                return RedirectToAction("Edit", "Profesionales", new { id });

            // Por defecto: no editable o es usuario genérico
            return RedirectToAction("EditGenerico", new { id });
        }

        // Método auxiliar para validar existencia de una persona por ID
        private bool PersonaExists(int id)
        {
            return _context.Personas.Any(e => e.Id == id);
        }
    }
}
