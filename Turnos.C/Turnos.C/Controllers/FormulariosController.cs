using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Turnos.C.Data;
using Turnos.C.Helpers;
using Turnos.C.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Turnos.C.Controllers
{
    public class FormulariosController : Controller
    {
        private readonly TurnosContext _context;

        public FormulariosController(TurnosContext context)
        {
            _context = context;
        }

        // GET: Formularios
        public async Task<IActionResult> Index()
        {
            IQueryable<Formulario> query = _context.Formularios.Include(f => f.Paciente);

            if (User.IsInRole(Config.AdminRolName) || User.IsInRole(Config.ProfesionalRolName))
            {
                return View(await query.ToListAsync());
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userId, out int pacienteId))
                    return Unauthorized();

                query = query.Where(f => f.PacienteId == pacienteId);
                return View(await query.ToListAsync());
            } 
            else
            {
                var lista = HttpContext.Session.GetString("FormulariosAnonimos");
                if (string.IsNullOrEmpty(lista))
                    return RedirectToAction(nameof(Create));

                var ids = lista.Split(',').Select(int.Parse).ToList();
                query = query.Where(f => ids.Contains(f.Id));
            }

            return View(await query.ToListAsync());
        }
        // GET: Formularios/Details/
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var formulario = await _context.Formularios
                .Include(f => f.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (formulario == null)
            { 
                if (!User.IsInRole(Config.AdminRolName) && !User.IsInRole(Config.ProfesionalRolName))
                {
                    //Para no indicar si realmente existe dicho formulario reuso el mensaje
                    TempData["Mensaje"] = "No tiene acceso a ver detalles de formularios que no son suyos.";
                } else
                {
                    TempData["Mensaje"] = $"No existe formulario con id {id}";
                }
                return RedirectToAction("Index"); ;
            }
            if (User.IsInRole(Config.AdminRolName) || User.IsInRole(Config.ProfesionalRolName))
            {
                if (User.IsInRole(Config.ProfesionalRolName) && (!formulario.Leido)) 
                    {
                    formulario.Leido = true;
                    _context.Update(formulario);
                    await _context.SaveChangesAsync();
                }
            }
            else if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userId, out int pacienteId) || formulario.PacienteId != pacienteId) 
                {
                    TempData["Mensaje"] = "No tiene acceso a ver detalles de formularios que no son suyos.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                var lista = HttpContext.Session.GetString("FormulariosAnonimos");
                var ids = string.IsNullOrEmpty(lista) ? new List<int>() : lista.Split(',').Select(int.Parse).ToList();
                if (!ids.Contains(formulario.Id)) 
                { 
                    TempData["Mensaje"] = "No tiene acceso a ver detalles de formularios que no son suyos.";
                    return RedirectToAction("Index");
                }
            }

            return View(formulario);
        }

        // GET: Formularios/Create
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole(Config.ProfesionalRolName))
            {
                return Forbid();
            }
                ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;

            if (User.IsInRole(Config.AdminRolName))
            {
                var pacientes = await _context.Pacientes
                    .Select(p => new { p.Id, NombreCompleto = p.Nombre + " " + p.Apellido })
                    .ToListAsync();

                ViewBag.Pacientes = new SelectList(pacientes, "Id", "NombreCompleto");
            }
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (int.TryParse(userId, out int pacienteId))
                {
                    bool tieneFormularios = await _context.Formularios.AnyAsync(f => f.PacienteId == pacienteId);
                    ViewBag.HasForms = tieneFormularios;
                }
                else
                {
                    ViewBag.HasForms = false;
                }
            }
            else
            {
                var formularios = HttpContext.Session.GetString("FormulariosAnonimos");
                ViewBag.HasForms = !string.IsNullOrEmpty(formularios);
            }

            return View();
        }
        // POST: Formularios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Formulario formulario)
        {
            if (User.IsInRole(Config.AdminRolName) && formulario.PacienteId.HasValue)
            {
                var paciente = await _context.Pacientes.FindAsync(formulario.PacienteId.Value);
                if (paciente != null)
                {
                    formulario.Email = paciente.Email;
                    formulario.Nombre = paciente.Nombre;
                    formulario.Apellido = paciente.Apellido;
                    formulario.DNI = paciente.DNI;

                    ModelState.Remove(nameof(Formulario.Email));
                    ModelState.Remove(nameof(Formulario.Nombre));
                    ModelState.Remove(nameof(Formulario.Apellido));
                    ModelState.Remove(nameof(Formulario.DNI));
                }
            }
            else if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userId, out int pacienteId))
                    return Unauthorized();

                var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.Id == pacienteId);
                if (paciente == null)
                    return NotFound();

                formulario.PacienteId = paciente.Id;
                formulario.Email = paciente.Email;
                formulario.Nombre = paciente.Nombre;
                formulario.Apellido = paciente.Apellido;
                formulario.DNI = paciente.DNI;

                ModelState.Remove(nameof(Formulario.Email));
                ModelState.Remove(nameof(Formulario.Nombre));
                ModelState.Remove(nameof(Formulario.Apellido));
                ModelState.Remove(nameof(Formulario.DNI));
            }

            formulario.Fecha = DateTime.Now;

            if (ModelState.IsValid)
            {
                formulario.Leido = false;
                _context.Add(formulario);
                await _context.SaveChangesAsync();

                if (!User.Identity.IsAuthenticated)
                {
                    var lista = HttpContext.Session.GetString("FormulariosAnonimos");
                    var ids = string.IsNullOrEmpty(lista) ? new List<int>() : lista.Split(',').Select(int.Parse).ToList();
                    ids.Add(formulario.Id);
                    HttpContext.Session.SetString("FormulariosAnonimos", string.Join(",", ids));
                }

                return RedirectToAction(nameof(Details), new { id = formulario.Id });
            }

            ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;
            return View(formulario);
        }

        // GET: Formularios/Edit/5
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Index));
            }


            var formulario = await _context.Formularios.FindAsync(id);
            if (formulario == null)
            {
                TempData["Mensaje"] = $"No existe formulario con id {id}";
                return RedirectToAction(nameof(Index));
            }

            return View(formulario);
        }

        // POST: Formularios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Fecha,Leido,Titulo,Mensaje")] Formulario formularioEditado)
        {
            if (id != formularioEditado.Id)
                return NotFound();

            var formulario = await _context.Formularios.FindAsync(id);
            if (formulario == null)
                return NotFound();

            formulario.Fecha = formularioEditado.Fecha;
            formulario.Leido = formularioEditado.Leido;
            formulario.Titulo = formularioEditado.Titulo;
            formulario.Mensaje = formularioEditado.Mensaje;

            ModelState.Remove(nameof(Formulario.Email));
            ModelState.Remove(nameof(Formulario.Nombre));
            ModelState.Remove(nameof(Formulario.Apellido));
            ModelState.Remove(nameof(Formulario.DNI));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(formulario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Formularios.Any(e => e.Id == formulario.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(formulario);
        }

        // GET: Formularios/Delete/5
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var formulario = await _context.Formularios
                .Include(f => f.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (formulario == null)
                return NotFound();

            return View(formulario);
        }

        // POST: Formularios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var formulario = await _context.Formularios.FindAsync(id);
            if (formulario != null)
            {
                _context.Formularios.Remove(formulario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FormularioExists(int id)
        {
            return _context.Formularios.Any(e => e.Id == id);
        }
    }
}
