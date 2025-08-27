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
    public class PacientesController : Controller
    {
        private readonly TurnosContext _context;
        private readonly UserManager<Persona> _userManager;

        public PacientesController(TurnosContext context, UserManager<Persona> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = Config.ProfesionalRolName + "," + Config.AdminRolName)]
        public async Task<IActionResult> Index()
        {
            var pacientes = await _context.Pacientes
                .Include(p => p.Cobertura)
                .ToListAsync();

            return View(pacientes);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var paciente = await _context.Pacientes
                .Include(p => p.Cobertura)
                .Include(p => p.Telefonos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paciente == null)
                return NotFound();

            return View(paciente);
        }

        [Authorize(Roles = Config.AdminRolName)]
        public async Task<IActionResult> Create()
        {
            var coberturas = await _context.Coberturas
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Prestadora} - {c.NumeroCredencial}"
                })
                .ToListAsync();

            var vm = new PacienteViewModel
            {
                Coberturas = coberturas,
                Telefonos = new List<TelefonoViewModel> { new TelefonoViewModel() }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Config.AdminRolName)]
        public async Task<IActionResult> Create(PacienteViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NuevaCoberturaNumeroCredencial))
                ModelState.AddModelError(nameof(model.NuevaCoberturaNumeroCredencial), "Debe ingresar el número de credencial.");

            if (!model.NuevaCoberturaPrestadora.HasValue)
                ModelState.AddModelError(nameof(model.NuevaCoberturaPrestadora), "Debe seleccionar la prestadora.");

            if (!ModelState.IsValid)
                return View(model);

            var paciente = new Paciente
            {
                Email = model.Email,
                UserName = model.Email,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                DNI = model.DNI,
                Telefonos = model.Telefonos
                    .Where(t => !string.IsNullOrWhiteSpace(t.Numero) && !string.IsNullOrWhiteSpace(t.Prefijo))
                    .Select(t => new Telefono
                    {
                        Numero = t.Numero,
                        Prefijo = t.Prefijo,
                        TipoTelefono = t.TipoTelefono
                    }).ToList(),
                Direccion = new Direccion
                {
                    Calle = model.Direccion.Calle,
                    Altura = model.Direccion.Altura,
                    Piso = model.Direccion.Piso,
                    Departamento = model.Direccion.Departamento
                }
            };

            var result = await _userManager.CreateAsync(paciente, Config.PasswordGenerica);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                return View(model);
            }

            await _userManager.AddToRoleAsync(paciente, Config.PacienteRolName);

            var cobertura = new Cobertura
            {
                NumeroCredencial = model.NuevaCoberturaNumeroCredencial,
                Prestadora = model.NuevaCoberturaPrestadora.Value,
                PacienteId = paciente.Id
            };

            _context.Coberturas.Add(cobertura);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                SqlException innerException = dbEx.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {

                    ModelState.AddModelError(nameof(model.NuevaCoberturaNumeroCredencial),
                        "Ya existe una cobertura registrada con ese número de credencial.");

                    ModelState.AddModelError(nameof(model.DNI),
                        "Ya existe una persona registrada con ese DNI.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, dbEx.Message);
                }

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = Config.AdminRolName + "," + Config.PacienteRolName)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            bool esAdmin = User.IsInRole(Config.AdminRolName);

            if (!esAdmin && user.Id != id)
                return Forbid();

            var paciente = await _context.Pacientes
                .Include(p => p.Cobertura)
                .Include(p => p.Telefonos)
                .Include(p => p.Direccion)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paciente == null)
                return NotFound();

            var coberturas = await _context.Coberturas
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Prestadora} - {c.NumeroCredencial}"
                })
                .ToListAsync();

            var vm = new PacienteViewModel
            {
                Id = paciente.Id,
                Nombre = paciente.Nombre,
                Apellido = paciente.Apellido,
                Email = paciente.Email,
                DNI = paciente.DNI,
                CoberturaId = paciente.Cobertura?.Id,
                NuevaCoberturaNumeroCredencial = paciente.Cobertura?.NumeroCredencial,
                NuevaCoberturaPrestadora = paciente.Cobertura?.Prestadora,
                Coberturas = coberturas,
                Telefonos = paciente.Telefonos?.Select(t => new TelefonoViewModel
                {
                    Id = t.Id,
                    Numero = t.Numero,
                    Prefijo = t.Prefijo,
                    TipoTelefono = t.TipoTelefono
                }).ToList() ?? new List<TelefonoViewModel> { new TelefonoViewModel() },
                Direccion = paciente.Direccion != null
                    ? new DireccionViewModel
                    {
                        Calle = paciente.Direccion.Calle,
                        Altura = paciente.Direccion.Altura,
                        Piso = paciente.Direccion.Piso,
                        Departamento = paciente.Direccion.Departamento
                    }
                    : new DireccionViewModel()
            };

            ViewData["FormAction"] = "Edit";

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Config.AdminRolName + "," + Config.PacienteRolName)]
        public async Task<IActionResult> Edit(int id, PacienteViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            bool esAdmin = User.IsInRole(Config.AdminRolName);

            if (!esAdmin && user.Id != id)
                return Forbid();

            if (!ModelState.IsValid)
            {
                await CargarCoberturas(model);
                ViewData["FormAction"] = "Edit";
                return View(model);
            }

            var paciente = await ObtenerPacienteConRelaciones(id);
            if (paciente == null)
                return NotFound();

            ProtegerDatosPacienteSiEsPaciente(paciente, model, esAdmin);
            ActualizarDatosBasicos(paciente, model);
            ActualizarCobertura(paciente, model);
            ActualizarDireccion(paciente, model);
            ActualizarTelefonos(paciente, model);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                SqlException innerException = dbEx.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    ModelState.AddModelError(nameof(model.DNI),
                        "Ya existe una persona registrada con ese DNI.");

                    ModelState.AddModelError(nameof(model.NuevaCoberturaNumeroCredencial),
                        "Ya existe una cobertura registrada con ese número de credencial.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, dbEx.Message);
                }

                await CargarCoberturas(model);
                ViewData["FormAction"] = "Edit";
                return View(model);
            }

            return esAdmin
                ? RedirectToAction(nameof(Index))
                : RedirectToAction("Index", "Home");
        }

        private async Task<Paciente> ObtenerPacienteConRelaciones(int id)
        {
            return await _context.Pacientes
                .Include(p => p.Cobertura)
                .Include(p => p.Telefonos)
                .Include(p => p.Direccion)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        private async Task CargarCoberturas(PacienteViewModel model)
        {
            model.Coberturas = await _context.Coberturas
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Prestadora} - {c.NumeroCredencial}"
                }).ToListAsync();
        }

        private void ProtegerDatosPacienteSiEsPaciente(Paciente paciente, PacienteViewModel model, bool esAdmin)
        {
            if (esAdmin) return;

            if (!string.IsNullOrWhiteSpace(paciente.Nombre))
                model.Nombre = paciente.Nombre;
            if (!string.IsNullOrWhiteSpace(paciente.Apellido))
                model.Apellido = paciente.Apellido;
            if (!string.IsNullOrWhiteSpace(paciente.Email))
                model.Email = paciente.Email;
            if (!string.IsNullOrWhiteSpace(paciente.DNI))
                model.DNI = paciente.DNI;
        }

        private void ActualizarDatosBasicos(Paciente paciente, PacienteViewModel model)
        {
            paciente.Nombre = model.Nombre;
            paciente.Apellido = model.Apellido;
            paciente.Email = model.Email;
            paciente.UserName = model.Email;
            paciente.DNI = model.DNI;
        }

        private void ActualizarCobertura(Paciente paciente, PacienteViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NuevaCoberturaNumeroCredencial) || !model.NuevaCoberturaPrestadora.HasValue)
                return;
            if (paciente.Cobertura != null)
            {
                paciente.Cobertura.NumeroCredencial = model.NuevaCoberturaNumeroCredencial;
                paciente.Cobertura.Prestadora = model.NuevaCoberturaPrestadora.Value;
            }
            else
            {
                paciente.Cobertura = new Cobertura
                {
                    NumeroCredencial = model.NuevaCoberturaNumeroCredencial,
                    Prestadora = model.NuevaCoberturaPrestadora.Value,
                    PacienteId = paciente.Id
                };
                _context.Coberturas.Add(paciente.Cobertura);
            }
        }

        private void ActualizarDireccion(Paciente paciente, PacienteViewModel model)
        {
            if (paciente.Direccion != null)
            {
                paciente.Direccion.Calle = model.Direccion.Calle;
                paciente.Direccion.Altura = model.Direccion.Altura;
                paciente.Direccion.Piso = model.Direccion.Piso;
                paciente.Direccion.Departamento = model.Direccion.Departamento;
            }
            else
            {
                paciente.Direccion = new Direccion
                {
                    Calle = model.Direccion.Calle,
                    Altura = model.Direccion.Altura,
                    Piso = model.Direccion.Piso,
                    Departamento = model.Direccion.Departamento
                };
            }
        }

        private void ActualizarTelefonos(Paciente paciente, PacienteViewModel model)
        {
            _context.Telefonos.RemoveRange(paciente.Telefonos);

            paciente.Telefonos = model.Telefonos
                .Where(t => !string.IsNullOrWhiteSpace(t.Numero) && !string.IsNullOrWhiteSpace(t.Prefijo))
                .Select(t => new Telefono
                {
                    Numero = t.Numero,
                    Prefijo = t.Prefijo,
                    TipoTelefono = t.TipoTelefono,
                    PersonaId = paciente.Id
                }).ToList();
        }

        [Authorize(Roles = Config.PacienteRolName)]
        public async Task<IActionResult> EditarMiPerfil()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user is not Paciente paciente)
                return NotFound();

            return RedirectToAction("Edit", new { id = paciente.Id });
        }
    }
}

