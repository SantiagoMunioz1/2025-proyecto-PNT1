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
    public class CoberturasController : Controller
    {
        private readonly TurnosContext _context;

        public CoberturasController(TurnosContext context)
        {
            _context = context;
        }
        [Authorize(Roles = $"{Config.AdminRolName}")]
        // GET: Coberturas
        public async Task<IActionResult> Index()
        {
            var turnosContext = _context.Coberturas.Include(c => c.Paciente);
            return View(await turnosContext.ToListAsync());
        }
        [Authorize(Roles = $"{Config.AdminRolName}")]
        // GET: Coberturas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cobertura = await _context.Coberturas
                .Include(c => c.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cobertura == null)
            {
                return NotFound();
            }

            return View(cobertura);
        }
        [Authorize(Roles = $"{Config.AdminRolName}")]
        // GET: Coberturas/Create
        public IActionResult Create()
        {
            ViewData["PacienteId"] = new SelectList(_context.Pacientes, "Id", "Apellido");
            return View();
        }

        // POST: Coberturas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NumeroCredencial,Prestadora,PacienteId")] Cobertura cobertura)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(cobertura);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbex)
                {
                    SqlException innerException = dbex.InnerException as SqlException;
                    if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                    {
                        ModelState.AddModelError("NumeroCredencial", "Ya existe una cobertura con ese número de credencial.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbex.Message);
                    }
                }
            }

            ViewData["PacienteId"] = new SelectList(_context.Pacientes, "Id", "Apellido", cobertura.PacienteId);
            return View(cobertura);
        }


        [Authorize(Roles = $"{Config.AdminRolName}")]
        // GET: Coberturas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cobertura = await _context.Coberturas.FindAsync(id);
            if (cobertura == null)
            {
                return NotFound();
            }
            ViewData["PacienteId"] = new SelectList(_context.Pacientes, "Id", "Apellido", cobertura.PacienteId);
            return View(cobertura);
        }

        // POST: Coberturas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NumeroCredencial,Prestadora,PacienteId")] Cobertura cobertura)
        {
            if (id != cobertura.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cobertura);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoberturaExists(cobertura.Id))
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
                        ModelState.AddModelError("NumeroCredencial", "Ya existe una cobertura con ese número de credencial.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbex.Message);
                    }
                }
            }

            ViewData["PacienteId"] = new SelectList(_context.Pacientes, "Id", "Apellido", cobertura.PacienteId);
            return View(cobertura);
        }



        [Authorize(Roles = $"{Config.AdminRolName}")]
        // GET: Coberturas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cobertura = await _context.Coberturas
                .Include(c => c.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cobertura == null)
            {
                return NotFound();
            }

            return View(cobertura);
        }

        // POST: Coberturas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cobertura = await _context.Coberturas.FindAsync(id);
            if (cobertura != null)
            {
                _context.Coberturas.Remove(cobertura);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CoberturaExists(int id)
        {
            return _context.Coberturas.Any(e => e.Id == id);
        }
    }
}
