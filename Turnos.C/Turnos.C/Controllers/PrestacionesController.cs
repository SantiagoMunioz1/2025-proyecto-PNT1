using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PrestacionesController : Controller
    {
        private readonly TurnosContext _context;

        public PrestacionesController(TurnosContext context)
        {
            _context = context;
        }

        // GET: Prestaciones
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Prestaciones.ToListAsync());
        }

        // GET: Prestaciones/Details/5
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prestacion = await _context.Prestaciones
                .Include(p => p.Profesionales)  // Solo esto
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prestacion == null)
            {
                return NotFound();
            }

            prestacion.Profesionales = prestacion.Profesionales
                .OrderBy(p => p.Apellido)
                .ThenBy(p => p.Nombre)
                .ToList();

            return View(prestacion);
        }

        // GET: Prestaciones/Create
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Prestaciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion,Duracion,Precio")] Prestacion prestacion)
        {
            if (prestacion.Duracion <= 0)
                ModelState.AddModelError("Duracion", "La duración debe ser mayor a cero.");
            if (prestacion.Precio <= 0)
                ModelState.AddModelError("Precio", "El precio debe ser mayor a cero.");

            if (!ModelState.IsValid)
                return View(prestacion);

            _context.Add(prestacion);

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException is SqlException innerException &&
                    (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    ModelState.AddModelError(nameof(prestacion.Nombre),
                        "Ya existe una prestación con ese nombre.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, dbEx.Message);
                }

                return View(prestacion);
            }
        }


        // GET: Prestaciones/Edit/5
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prestacion = await _context.Prestaciones.FindAsync(id);
            if (prestacion == null)
            {
                return NotFound();
            }
            return View(prestacion);
        }

        // POST: Prestaciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Duracion,Precio")] Prestacion prestacion)
        {
            if (id != prestacion.Id)
                return NotFound();

            prestacion.Nombre = prestacion.Nombre.Trim();
            prestacion.Descripcion = prestacion.Descripcion?.Trim();

            if (prestacion.Duracion <= 0)
                ModelState.AddModelError("Duracion", "La duración debe ser mayor a cero.");

            if (prestacion.Precio <= 0)
                ModelState.AddModelError("Precio", "El precio debe ser mayor a cero.");

            if (!ModelState.IsValid)
                return View(prestacion);

            try
            {
                _context.Update(prestacion);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PrestacionExists(prestacion.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException dbex)
            {
                SqlException innerException = dbex.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    ModelState.AddModelError("Nombre", "Ya existe una prestación con ese nombre.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, dbex.Message);
                }

                return View(prestacion);
            }

            return RedirectToAction(nameof(Index));
        }



        // GET: Prestaciones/Delete/5
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prestacion = await _context.Prestaciones
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prestacion == null)
            {
                return NotFound();
            }

            return View(prestacion);
        }

        // POST: Prestaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prestacion = await _context.Prestaciones
                .Include(p => p.Profesionales)
                .ThenInclude(prof => prof.Turnos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prestacion == null)
                return NotFound();

            bool tieneTurnos = prestacion.Profesionales.Any(p => p.Turnos != null && p.Turnos.Any());

            if (tieneTurnos)
            {
                TempData["Error"] = "No se puede eliminar la prestación porque hay profesionales con turnos asignados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Prestaciones.Remove(prestacion);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "La prestación y sus profesionales vinculados fueron eliminados correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool PrestacionExists(int id)
        {
            return _context.Prestaciones.Any(e => e.Id == id);
        }

        public async Task<IActionResult> VerPrestaciones()
        {
            var prestaciones = await _context.Prestaciones.Include(p => p.Profesionales).ToListAsync();

            foreach (var p in prestaciones)
                p.Profesionales = p.Profesionales
                    .OrderBy(pr => pr.Apellido)
                    .ThenBy(pr => pr.Nombre)
                    .ToList();

            return View(prestaciones);
        }
    }
}
