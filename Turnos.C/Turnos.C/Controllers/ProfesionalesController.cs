using System;
using System.Collections.Generic;
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
    public class ProfesionalesController : Controller
    {
        private readonly TurnosContext _context;
        private readonly UserManager<Persona> _userManager;
        private readonly RoleManager<Rol> _roleManager;

        public ProfesionalesController(
            TurnosContext context,
            UserManager<Persona> userManager,
            RoleManager<Rol> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Index()
        {
            var profesionales = await _context.Profesionales
                .Include(p => p.Prestacion)
                .ToListAsync();

            return View(profesionales);
        }
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var profesional = await _context.Profesionales
                .Include(p => p.Prestacion)
                .Include(p => p.Telefonos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (profesional == null)
                return NotFound();

            return View(profesional);
        }

        [Authorize(Roles = Config.AdminRolName)]
        public async Task<IActionResult> Create()
        {
            var prestaciones = await _context.Prestaciones
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Nombre })
                .ToListAsync();

            var vm = new ProfesionalViewModel
            {
                Prestaciones = prestaciones,
                HoraInicio = new TimeOnly(9, 0),
                HoraFin = new TimeOnly(17, 0),
                Telefonos = new() { new TelefonoViewModel() }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Config.AdminRolName)]
        public async Task<IActionResult> Create(ProfesionalViewModel model)
        {
            bool seleccionoPrestacionExistente = model.PrestacionId > 0;
            bool quiereCrearPrestacionNueva = !string.IsNullOrWhiteSpace(model.NuevaPrestacionNombre);

            if (seleccionoPrestacionExistente && quiereCrearPrestacionNueva)
                ModelState.AddModelError("", "Seleccione una prestación existente o cree una nueva, pero no ambas.");

            if (!seleccionoPrestacionExistente && !quiereCrearPrestacionNueva)
                ModelState.AddModelError("", "Debe seleccionar una prestación o ingresar una nueva.");

            if (quiereCrearPrestacionNueva)
            {
                if (string.IsNullOrWhiteSpace(model.NuevaPrestacionDescripcion))
                    ModelState.AddModelError(nameof(model.NuevaPrestacionDescripcion), "Debe ingresar una descripción para la nueva prestación.");
                if (model.NuevaPrestacionDuracion is null || model.NuevaPrestacionDuracion <= 0)
                    ModelState.AddModelError(nameof(model.NuevaPrestacionDuracion), "La duración debe ser mayor a 0.");
                if (model.NuevaPrestacionPrecio is null || model.NuevaPrestacionPrecio <= 0)
                    ModelState.AddModelError(nameof(model.NuevaPrestacionPrecio), "El precio debe ser mayor a 0.");
            }

            if (!ModelState.IsValid)
            {
                model.Prestaciones = await ObtenerSelectPrestaciones();
                return View(model);
            }

            try
            {
                int prestacionId;
                if (quiereCrearPrestacionNueva)
                {
                    var nuevaPrestacion = new Prestacion
                    {
                        Nombre = model.NuevaPrestacionNombre.Trim(),
                        Descripcion = model.NuevaPrestacionDescripcion.Trim(),
                        Duracion = model.NuevaPrestacionDuracion.Value,
                        Precio = model.NuevaPrestacionPrecio.Value
                    };

                    _context.Prestaciones.Add(nuevaPrestacion);
                    await _context.SaveChangesAsync();
                    prestacionId = nuevaPrestacion.Id;
                }
                else
                {
                    prestacionId = model.PrestacionId.Value;
                }

                var profesional = new Profesional
                {
                    Email = model.Email,
                    UserName = model.Email,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    DNI = model.DNI,
                    PrestacionId = prestacionId,
                    HoraInicio = model.HoraInicio,
                    HoraFin = model.HoraFin,
                    Telefonos = model.Telefonos
                        .Where(t => !string.IsNullOrWhiteSpace(t.Numero) && !string.IsNullOrWhiteSpace(t.Prefijo))
                        .Select(t => new Telefono
                        {
                            Numero = t.Numero,
                            Prefijo = t.Prefijo,
                            TipoTelefono = t.TipoTelefono
                        }).ToList()
                };

                var res = await _userManager.CreateAsync(profesional, Config.PasswordGenerica);
                if (!res.Succeeded)
                {
                    foreach (var e in res.Errors)
                        ModelState.AddModelError("", e.Description);

                    model.Prestaciones = await ObtenerSelectPrestaciones();
                    return View(model);
                }

                await _userManager.AddToRoleAsync(profesional, Config.ProfesionalRolName);

                var matricula = new Matricula
                {
                    NumeroMatricula = model.NumeroMatricula,
                    Provincia = model.Provincia,
                    Tipo = model.Tipo,
                    ProfesionalId = profesional.Id
                };

                _context.Matriculas.Add(matricula);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (DbUpdateException dbex)
            {
                SqlException innerException = dbex.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    if (innerException.Message.Contains("NumeroMatricula"))
                        ModelState.AddModelError(nameof(model.NumeroMatricula), "Ya existe una matrícula con ese número.");
                    else if (innerException.Message.Contains("DNI"))
                        ModelState.AddModelError(nameof(model.DNI), "Ya existe una persona con ese DNI.");
                    else if (innerException.Message.Contains("Nombre"))
                        ModelState.AddModelError(nameof(model.NuevaPrestacionNombre), "Ya existe una prestación con ese nombre.");
                    else
                        ModelState.AddModelError(string.Empty, "Error de duplicidad en los datos ingresados.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, dbex.Message);
                }

                model.Prestaciones = await ObtenerSelectPrestaciones();
                return View(model);
            }
        }

        [Authorize(Roles = Config.AdminRolName)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var profesional = await _context.Profesionales
                .Include(p => p.Matricula)
                .Include(p => p.Prestacion)
                .Include(p => p.Telefonos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profesional == null)
                return NotFound();

            var prestaciones = await _context.Prestaciones
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Nombre })
                .ToListAsync();

            var vm = new ProfesionalViewModel
            {
                Id = profesional.Id,
                Nombre = profesional.Nombre,
                Apellido = profesional.Apellido,
                Email = profesional.Email,
                DNI = profesional.DNI,
                PrestacionId = profesional.PrestacionId,
                Prestaciones = prestaciones,
                HoraInicio = profesional.HoraInicio,
                HoraFin = profesional.HoraFin,
                NumeroMatricula = profesional.Matricula.NumeroMatricula,
                Provincia = profesional.Matricula.Provincia,
                Tipo = profesional.Matricula.Tipo,
                Telefonos = profesional.Telefonos?.Select(t => new TelefonoViewModel
                {
                    Id = t.Id,
                    Numero = t.Numero,
                    Prefijo = t.Prefijo,
                    TipoTelefono = t.TipoTelefono
                }).ToList() ?? new() { new TelefonoViewModel() }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProfesionalViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            bool seleccionoPrestacionExistente = model.PrestacionId > 0;
            bool quiereCrearPrestacionNueva = !string.IsNullOrWhiteSpace(model.NuevaPrestacionNombre);

            if (seleccionoPrestacionExistente && quiereCrearPrestacionNueva)
                ModelState.AddModelError("", "Seleccione una prestación existente o cree una nueva, pero no ambas.");

            if (!seleccionoPrestacionExistente && !quiereCrearPrestacionNueva)
                ModelState.AddModelError("", "Debe seleccionar una prestación o ingresar una nueva.");

            if (quiereCrearPrestacionNueva)
            {
                if (string.IsNullOrWhiteSpace(model.NuevaPrestacionDescripcion))
                    ModelState.AddModelError(nameof(model.NuevaPrestacionDescripcion), "Debe ingresar una descripción para la nueva prestación.");
                if (model.NuevaPrestacionDuracion is null || model.NuevaPrestacionDuracion <= 0)
                    ModelState.AddModelError(nameof(model.NuevaPrestacionDuracion), "La duración debe ser mayor a 0.");
                if (model.NuevaPrestacionPrecio is null || model.NuevaPrestacionPrecio <= 0)
                    ModelState.AddModelError(nameof(model.NuevaPrestacionPrecio), "El precio debe ser mayor a 0.");
            }

            if (!ModelState.IsValid)
            {
                model.Prestaciones = await ObtenerSelectPrestaciones();

                return View(model);
            }

            var profesional = await _context.Profesionales
                .Include(p => p.Matricula)
                .Include(p => p.Telefonos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profesional == null)
                return NotFound();

            int prestacionId;
            if (quiereCrearPrestacionNueva)
            {
                var nuevaPrestacion = new Prestacion
                {
                    Nombre = model.NuevaPrestacionNombre.Trim(),
                    Descripcion = model.NuevaPrestacionDescripcion.Trim(),
                    Duracion = model.NuevaPrestacionDuracion.Value,
                    Precio = model.NuevaPrestacionPrecio.Value
                };

                _context.Prestaciones.Add(nuevaPrestacion);
                await _context.SaveChangesAsync();
                prestacionId = nuevaPrestacion.Id;
            }
            else
            {
                prestacionId = model.PrestacionId.Value;
            }

            profesional.Nombre = model.Nombre;
            profesional.Apellido = model.Apellido;
            profesional.Email = model.Email;
            profesional.UserName = model.Email;
            profesional.DNI = model.DNI;
            profesional.HoraInicio = model.HoraInicio;
            profesional.HoraFin = model.HoraFin;
            profesional.PrestacionId = prestacionId;

            profesional.Matricula.NumeroMatricula = model.NumeroMatricula;
            profesional.Matricula.Provincia = model.Provincia;
            profesional.Matricula.Tipo = model.Tipo;

            _context.Telefonos.RemoveRange(profesional.Telefonos);

            profesional.Telefonos = model.Telefonos
                .Where(t => !string.IsNullOrWhiteSpace(t.Numero) && !string.IsNullOrWhiteSpace(t.Prefijo))
                .Select(t => new Telefono
                {
                    Numero = t.Numero,
                    Prefijo = t.Prefijo,
                    TipoTelefono = t.TipoTelefono,
                    PersonaId = profesional.Id
                }).ToList();

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = Config.AdminRolName)]
        public IActionResult Delete(int id, string returnUrl = null)
        {
            // Redireccion al metodo central delete de PersonasController
            return RedirectToAction("Delete", "Personas", new { id, returnUrl });
        }

        public async Task<IActionResult> VerProfesionales()
        {
            var profesionales = await _context.Profesionales
            .Include(p => p.Prestacion).OrderBy(pr => pr.Prestacion.Nombre).ThenBy(pr => pr.Nombre)
            .ToListAsync();

            return View(profesionales);
        }

        private async Task<List<SelectListItem>> ObtenerSelectPrestaciones()
        {
            return await _context.Prestaciones
                .OrderBy(p => p.Nombre)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Nombre
                }).ToListAsync();
        }        
    }
}
