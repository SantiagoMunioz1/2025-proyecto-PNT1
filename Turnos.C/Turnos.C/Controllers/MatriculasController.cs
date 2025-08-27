using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Turnos.C.Data;
using Turnos.C.Helpers;
using Turnos.C.Models;

namespace Turnos.C.Controllers
{
    public class MatriculasController : Controller
    {
        private readonly TurnosContext _context;

        public MatriculasController(TurnosContext context)
        {
            _context = context;
        }
        // GET: Matriculas
        [Authorize(Roles = $"{Config.AdminRolName}, {Config.ProfesionalRolName}")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (User.IsInRole(Config.ProfesionalRolName))
            {
                var matriculasContext = _context.Matriculas.Include(t => t.Profesional)
                    .Where(t => t.Profesional.Id == int.Parse(userId)).ToListAsync();

                return View(await matriculasContext);
            }
            else
            {
                var matriculasContext = _context.Matriculas.Include(t => t.Profesional).ToListAsync();

                return View(await matriculasContext);
            }
        }

        // GET: Matriculas/Details/5
        [Authorize(Roles = $"{Config.AdminRolName}, {Config.ProfesionalRolName}")]
        public async Task<IActionResult> Details(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id == null)
            {
                return NotFound();
            }

            var matricula = await _context.Matriculas
                .Include(m => m.Profesional)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (matricula == null)
            {
                return NotFound();
            }

            if (User.IsInRole(Config.ProfesionalRolName) && matricula.ProfesionalId != int.Parse(userId))
            {
                return NotFound();
            }
            
            return View(matricula);
        }


        // GET: Matriculas/Edit/5
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var matricula = await _context.Matriculas
                .Include(m => m.Profesional)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matricula == null) return NotFound();

            ViewData["ProfesionalId"] = new SelectList(_context.Profesionales, "Id", "Apellido", matricula.ProfesionalId);

            var profesional = await _context.Profesionales.FindAsync(matricula.ProfesionalId);
            ViewBag.ProfesionalNombreCompleto = profesional?.NombreCompleto ?? "(Profesional desconocido)";

            return View(matricula);
        }

        // POST: Matriculas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{Config.AdminRolName}, {Config.ProfesionalRolName}")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NumeroMatricula,Provincia,Tipo,ProfesionalId")] Matricula matricula)
        {
            if (id != matricula.Id)
                return NotFound();

            if (string.IsNullOrWhiteSpace(matricula.NumeroMatricula))
                ModelState.AddModelError("NumeroMatricula", "Debe ingresar un número de matrícula.");

            if (matricula.ProfesionalId == 0)
                ModelState.AddModelError("ProfesionalId", "Debe seleccionar un profesional.");

            if (_context.Matriculas.Any(m => m.ProfesionalId == matricula.ProfesionalId && m.Id != matricula.Id))
                ModelState.AddModelError("ProfesionalId", "El profesional ya tiene una matrícula asignada.");

            if (!ModelState.IsValid)
            {
                ViewData["ProfesionalId"] = new SelectList(_context.Profesionales, "Id", "Apellido", matricula.ProfesionalId);
                return View(matricula);
            }

            try
            {
                _context.Update(matricula);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Matriculas.Any(m => m.Id == matricula.Id))
                    return NotFound();
                throw;
            }
            catch (DbUpdateException dbex)
            {
                SqlException innerException = dbex.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    ModelState.AddModelError("NumeroMatricula", "Ya existe una matrícula con ese número.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, dbex.Message);
                }

                ViewData["ProfesionalId"] = new SelectList(_context.Profesionales, "Id", "Apellido", matricula.ProfesionalId);
                return View(matricula);
            }

            return RedirectToAction("Details", new { id = matricula.Id });
        }
    }

}
