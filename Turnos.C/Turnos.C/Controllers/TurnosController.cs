using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Turnos.C.Data;
using Turnos.C.Models;
using System.Security.Claims;
using Turnos.C.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Turnos.C.Helpers;

namespace Turnos.C.Controllers
{
    public class TurnosController : Controller
    {
        private readonly TurnosContext _context;

        public TurnosController(TurnosContext context)
        {
            _context = context;
        }

        // GET: Turnos
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Index()
        {
            var turnos = await _context.Turnos
                .Include(t => t.Paciente)
                .Include(t => t.Profesional)
                .OrderBy(t => t.Fecha)
                .ToListAsync();

            var viewModel = turnos.Select(t => new TurnoAdminViewModel
            {
                Id = t.Id,
                Fecha = t.Fecha,
                PacienteNombre = t.Paciente.NombreCompleto,
                ProfesionalNombre = t.Profesional.NombreCompleto,
                Estado = EstadoTurnoHelper.ObtenerEstado(t),
                ClaseEstado = EstadoTurnoHelper.ObtenerClaseEstado(t)
            }).ToList();
            
            return View(viewModel);
        }

        [Authorize(Roles = Config.ProfesionalRolName)]
        public async Task<IActionResult> RegistroDeTurnosPorProfesional()
        {
            var profesionalId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var turnos = await _context.Turnos
                .Where(t => t.ProfesionalId == profesionalId)
                .Include(t => t.Paciente)
                .OrderByDescending(t => t.Fecha)
                .ToListAsync();

            var viewModel = turnos.Select(t => new TurnoProfesionalViewModel
            {
                Id = t.Id,
                Fecha = t.Fecha,
                FechaAlta = t.FechaAlta,
                PacienteNombre = $"{t.Paciente.Nombre} {t.Paciente.Apellido}",
                Estado = EstadoTurnoHelper.ObtenerEstado(t),
                ClaseEstado = EstadoTurnoHelper.ObtenerClaseEstado(t)
            });

            return View(viewModel);
        }


        [Authorize(Roles = $"{Config.AdminRolName},{Config.ProfesionalRolName}")]
        public async Task<IActionResult> Details(int? id, string returnUrl = null)
        {
            if (id == null)
                return NotFound();

            var turno = await _context.Turnos
                .Include(t => t.Paciente)
                .Include(t => t.Profesional)
                    .ThenInclude(p => p.Prestacion)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (turno == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole(Config.ProfesionalRolName) && turno.ProfesionalId != int.Parse(userId))
                return Forbid();

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index");

            return View(turno);
        }

        // GET: Turnos/Create
        [Authorize(Roles = Config.AdminRolName)]
        public IActionResult Create()
        {
            ViewData["PacienteId"] = new SelectList(_context.Pacientes, "Id", "Apellido");
            ViewData["ProfesionalId"] = new SelectList(_context.Profesionales, "Id", "Apellido");
            return View();
        }

        // POST: Turnos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = Config.AdminRolName)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Fecha,Confirmado,Activo,FechaAlta,PacienteId,ProfesionalId,DescripcionCancelacion")] Turno turno)
        {
            if (ModelState.IsValid)
            {
                _context.Add(turno);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PacienteId"] = new SelectList(_context.Pacientes, "Id", "Apellido", turno.PacienteId);
            ViewData["ProfesionalId"] = new SelectList(_context.Profesionales, "Id", "Apellido", turno.ProfesionalId);
            return View(turno);
        }

        // GET: Turnos/Edit/5
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null)
            {
                return NotFound();
            }
            ViewData["PacienteId"] = new SelectList(_context.Pacientes, "Id", "Apellido", turno.PacienteId);
            ViewData["ProfesionalId"] = new SelectList(_context.Profesionales, "Id", "Apellido", turno.ProfesionalId);
            return View(turno);
        }

        // POST: Turnos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = $"{Config.AdminRolName}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Fecha,Confirmado,Activo,FechaAlta,PacienteId,ProfesionalId,DescripcionCancelacion")] Turno turno)
        {
            if (id != turno.Id)
                return NotFound();

            // Validación: turno no debe ser en el pasado
            if (turno.Fecha <= DateTime.Now)
            {
                ModelState.AddModelError("Fecha", "No se puede asignar un turno en una fecha u horario pasado.");
            }

            // Validación: profesional debe existir y atender en ese horario
            var profesional = await _context.Profesionales
                .Include(p => p.Prestacion)
                .FirstOrDefaultAsync(p => p.Id == turno.ProfesionalId);

            if (profesional == null)
            {
                ModelState.AddModelError("ProfesionalId", "Profesional no encontrado.");
            }
            else
            {
                var hora = turno.Fecha.TimeOfDay;
                var duracion = profesional.Prestacion?.Duracion ?? 0;

                if (hora < profesional.HoraInicio.ToTimeSpan() || hora.Add(TimeSpan.FromMinutes(duracion)) > profesional.HoraFin.ToTimeSpan())
                {
                    ModelState.AddModelError("Fecha", "El horario seleccionado no está dentro del horario de atención del profesional.");
                }
            }

            // Validación: no se pueden superponer turnos ni tomar horarios que pisen otros turnos
            if (profesional != null)
            {
                var duracionTurno = TimeSpan.FromMinutes(profesional.Prestacion?.Duracion ?? 0);
                var inicioNuevo = turno.Fecha;
                var finNuevo = inicioNuevo.Add(duracionTurno);

                var superpuesto = await _context.Turnos.AnyAsync(t =>
                    t.Id != turno.Id &&
                    t.ProfesionalId == turno.ProfesionalId &&
                    t.Activo &&
                    (
                        //Si el turno existente empieza dentro del rango del nuevo turno
                        (t.Fecha >= inicioNuevo && t.Fecha < finNuevo) ||
                        // Si el turno existente empieza antes pero termina dentro o después del nuevo
                        (t.Fecha < inicioNuevo && t.Fecha.AddMinutes(profesional.Prestacion.Duracion) > inicioNuevo)
                    )
                );

                if (superpuesto)
                {
                    ModelState.AddModelError("Fecha", "El profesional ya tiene un turno asignado en ese rango horario.");
                }
            }


            // Si hay errores, volver al formulario
            if (!ModelState.IsValid)
            {
                ViewData["PacienteId"] = new SelectList(_context.Pacientes, "Id", "Apellido", turno.PacienteId);
                ViewData["ProfesionalId"] = new SelectList(_context.Profesionales, "Id", "Apellido", turno.ProfesionalId);
                return View(turno);
            }

            try
            {
                _context.Update(turno);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Turno modificado exitosamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TurnoExists(turno.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Turnos/Delete/5
        [Authorize(Roles = $"{Config.AdminRolName}")]
        public async Task<IActionResult> Delete(int? id, string returnUrl = null)
        {
            if (id == null)
            {
                return NotFound();
            }

            var turno = await _context.Turnos
                .Include(t => t.Paciente)
                .Include(t => t.Profesional)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (turno == null)
            {
                return NotFound();
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index");


            return View(turno);
        }

        [Authorize(Roles = Config.AdminRolName)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string returnUrl)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null)
                return NotFound();

            try
            {
                _context.Turnos.Remove(turno);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Turno eliminado exitosamente.";
            }
            catch (DbUpdateException dbex)
            {
                TempData["Error"] = "Error al eliminar el turno: " + dbex.Message;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TurnoExists(int id)
        {
            return _context.Turnos.Any(e => e.Id == id);
        }

        // GET: Turnos/SeleccionarPrestacion
        [Authorize]
        [Authorize(Roles = $"{Config.PacienteRolName},{Config.AdminRolName}")]
        public IActionResult SeleccionarPrestacion()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tieneTurnoActivo = _context.Pacientes.Include(p => p.Turnos)
                .Any(p => p.Id == int.Parse(userId) && p.Turnos.Any(t => t.Activo)); // Al no poder cronear y desactivar los turnos en fechas pasadas un paciente no puede pedir dos turnos a menos que un admin edite el turno actual.

            if (tieneTurnoActivo)
            {
                ViewBag.Mensaje = "Ya tiene un turno activo. No puede solicitar otro.";
            }

            var prestaciones = _context.Prestaciones.Where(p => p.Profesionales.Count() > 0).ToList();
            var model = new SeleccionarPrestacion
            {
                Prestaciones = new SelectList(prestaciones, "Id", "Nombre")
            };
            return View(model);
        }

        // POST: Turnos/SeleccionarPrestacion
        [HttpPost]
        public IActionResult SeleccionarPrestacion(SeleccionarPrestacion model)
        {
            if (!ModelState.IsValid) return View(model);

            int prestacionId = (int)model.PrestacionId;
            HttpContext.Session.SetInt32("PrestacionId", prestacionId);
            return RedirectToAction("SeleccionarProfesional"); 
        }

        // GET: Turnos/SeleccionarProfesional
        [Authorize]
        [Authorize(Roles = $"{Config.PacienteRolName},{Config.AdminRolName}")]
        public IActionResult SeleccionarProfesional()
        {
            var prestacionId = HttpContext.Session.GetInt32("PrestacionId");
            if (prestacionId == null) return RedirectToAction("SeleccionarPrestacion");

            var profesionales = _context.Profesionales.Where(p => p.PrestacionId == prestacionId).ToList();

            var model = new SeleccionarProfesional
            {
                Profesionales = new SelectList(profesionales, "Id", "NombreCompleto")
            };

            return View(model);
        }

        // POST: Turnos/SeleccionarProfesional
        [HttpPost]
        public IActionResult SeleccionarProfesional(SeleccionarProfesional model)
        {
            if (!ModelState.IsValid) return View(model);

            int profesionalId = (int)model.ProfesionalId;
            HttpContext.Session.SetInt32("ProfesionalId", profesionalId);
            return RedirectToAction("SeleccionarHorario");
        }

        // GET: Turnos/SeleccionarHorario
        [Authorize]
        [Authorize(Roles = $"{Config.PacienteRolName},{Config.AdminRolName}")]
        public IActionResult SeleccionarHorario()
        {
            var profesionalId = HttpContext.Session.GetInt32("ProfesionalId");
            var prestacionId = HttpContext.Session.GetInt32("PrestacionId");

            if (profesionalId == null || prestacionId == null)
                return RedirectToAction("SeleccionarPrestacion");

            var profesional = _context.Profesionales
                .Include(p => p.Prestacion)
                .Include(p => p.Turnos)
                .FirstOrDefault(p => p.Id == profesionalId);

            if (profesional == null)
                return RedirectToAction("SeleccionarPrestacion");

            var duracion = profesional.Prestacion.Duracion;

            var horariosDisponibles = GenerarHorariosDisponibles(profesional, duracion);

            var model = new SeleccionarHorario
            {
                HorariosDisponibles = horariosDisponibles
            };

            return View(model);
        }

        // POST: Turnos/SeleccionarHorario
        [HttpPost]
        public IActionResult SeleccionarHorario(SeleccionarHorario model)
        {
            if (!ModelState.IsValid) return View(model);

            HttpContext.Session.SetString("Horario", model.SelectedHorario.ToString("s"));

            return RedirectToAction("ConfirmarTurno");
        }

        // GET: Turnos/ConfirmarTurno
        [Authorize]
        [Authorize(Roles = $"{Config.PacienteRolName},{Config.AdminRolName}")]
        public IActionResult ConfirmarTurno()
        {
            var prestacionId = HttpContext.Session.GetInt32("PrestacionId");
            var profesionalId = HttpContext.Session.GetInt32("ProfesionalId");
            var horarioStr = HttpContext.Session.GetString("Horario");

            if (prestacionId == null || profesionalId == null || string.IsNullOrEmpty(horarioStr))
            {
                return RedirectToAction("SeleccionarPrestacion");
            }

            var horario = DateTime.Parse(horarioStr);

            var prestacion = _context.Prestaciones.FirstOrDefault(p => p.Id == prestacionId);
            var profesional = _context.Profesionales.FirstOrDefault(p => p.Id == profesionalId);

            var model = new ConfirmarTurno
            {
                PrestacionNombre = prestacion?.Nombre,
                ProfesionalNombre = profesional?.NombreCompleto ?? $"{profesional?.Nombre} {profesional?.Apellido}",
                FechaHora = horario
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = $"{Config.PacienteRolName},{Config.AdminRolName}")]
        public async Task<IActionResult> ConfirmarTurnoPost()
        {
            var profesionalId = HttpContext.Session.GetInt32("ProfesionalId");
            int pacienteId;
            if (User.IsInRole(Config.AdminRolName))
            {
                var pacienteIdSession = HttpContext.Session.GetInt32("PacienteId");
                if (pacienteIdSession == null) return RedirectToAction("SeleccionarPaciente");
                pacienteId = pacienteIdSession.Value;
            }
            else
            {
                pacienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            var horarioStr = HttpContext.Session.GetString("Horario");
            var horario = DateTime.Parse(horarioStr);

            // Validar que el horario no sea anterior al actual
            if (horario <= DateTime.Now)
            {
                TempData["ErrorTurno"] = "No se puede tomar un turno en una fecha u horario pasado.";
                return RedirectToAction("MisTurnos");
            }

            var paciente = await _context.Pacientes.FindAsync(pacienteId);
            if (paciente == null)
            {
                return Forbid(); // No es un paciente válido
            }

            // Ultima validacion de seguridad para verificar si ya tiene turno activo
            var yaTieneTurnoActivo = await _context.Turnos
                .AnyAsync(t => t.PacienteId == pacienteId && t.Activo);

            if (yaTieneTurnoActivo)
            {
                if (User.IsInRole(Config.AdminRolName))
                {
                    TempData["ErrorTurno"] = "El paciente ya tiene un turno activo y no puede solicitar otro";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorTurno"] = "Ya tenés un turno activo. No podés solicitar otro.";
                    return RedirectToAction("MisTurnos");
                }
            }

            // Este bloque valida desde el back que un usuario efectivamente no pueda sacar un turno en caso de que el
            // profesional ya tenga un turno asignado y activo en el mismo horario, pero plantea un problema frente
            // a los casos donde los profesionales cancelaron un turno, ya que la idea seria que ese turno quede bloqueado

            // Para resolver esto se propone agregar una nueva prop al modelo Turno que se llame "OrigenCancelacion"
            // donde mediante un Enum se puede determinar si quien cancelo el turno fue el propio paciente antes de
            // la fecha del turno en cuestion, o si fue cancelada por el profesional. De esta forma se puede validar
            // quien cancelo el turno y tener un handling mas preciso.

            var yaEstaOcupado = await _context.Turnos.AnyAsync(t =>
                t.ProfesionalId == profesionalId &&
                t.Fecha == horario &&
                t.Activo);

            
            if (yaEstaOcupado)
            {

                TempData["ErrorTurno"] = "El horario seleccionado ya fue ocupado. Por favor, seleccioná otro.";
                return User.IsInRole(Config.AdminRolName)
                    ? RedirectToAction("Index")
                    : RedirectToAction("MisTurnos");
            }

            var turno = new Turno
            {
                Fecha = horario,
                Confirmado = false,
                Activo = true,
                FechaAlta = DateTime.Now,
                PacienteId = pacienteId,
                ProfesionalId = (int)profesionalId,
                DescripcionCancelacion = null
            };

            if (ModelState.IsValid)
            {
                _context.Add(turno);
                await _context.SaveChangesAsync();

                // Limpiar atributos de la sesion 
                HttpContext.Session.Remove("PacienteId");
                HttpContext.Session.Remove("PrestacionId");
                HttpContext.Session.Remove("ProfesionalId");
                HttpContext.Session.Remove("Horario");

                TempData["Mensaje"] = "El turno fue agendado exitosamente.";

                return User.IsInRole(Config.AdminRolName)
                    ? RedirectToAction("Index")
                    : RedirectToAction("MisTurnos");
            }

            TempData["MensajeError"] = "Ocurrió un error al agendar el turno.";
            return View(turno);
        }

        // GET: Turnos/MisTurnos
        [Authorize(Roles = $"{Config.PacienteRolName}")]
        public IActionResult MisTurnos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var turnos = _context.Turnos
                .Include(t => t.Paciente)
                .Include(t => t.Profesional)
                    .ThenInclude(p => p.Prestacion)
                .Where(t => t.PacienteId == int.Parse(userId))
                .OrderByDescending(t => t.Fecha)
                .ToList();

            var viewModels = turnos.Select(t => new TurnoPacienteViewModel
            {
                Id = t.Id,
                Fecha = t.Fecha,
                PrestacionNombre = t.Profesional?.Prestacion?.Nombre,
                ProfesionalNombre = t.Profesional?.NombreCompleto,
                Estado = EstadoTurnoHelper.ObtenerEstado(t),
                ClaseEstado = EstadoTurnoHelper.ObtenerClaseEstado(t),
                Confirmado = t.Confirmado,
                DescripcionCancelacion = t.DescripcionCancelacion,
                PuedeCancelar = t.Activo && t.Fecha > DateTime.Now.AddHours(24)
            }).ToList();

            var turnoActivo = turnos.Find(t => t.Activo);

            if (turnoActivo != null && TempData.ContainsKey("TurnoExitoso"))
            {
                ViewBag.Mensaje = $"Turno agendado exitosamente!";
            }

            return View(viewModels);
        }

        [Authorize]
        [Authorize(Roles = Config.ProfesionalRolName)]
        public IActionResult ProximosTurnos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profesionalId = int.Parse(userId);
            DateTime hoy = DateTime.Today;

            var turnosDeHoy = _context.Turnos
                .Include(t => t.Paciente)
                .Where(t => t.ProfesionalId == profesionalId && t.Fecha.Date == hoy && t.Activo)
                .OrderBy(t => t.Fecha)
                .ToList();

            var proximosTurnos = _context.Turnos
                .Include(t => t.Paciente)
                .Where(t => t.ProfesionalId == profesionalId && t.Fecha.Date > hoy && t.Activo)
                .OrderBy(t => t.Fecha)
                .ToList();

            var hoyVM = turnosDeHoy.Select(t => new TurnoProfesionalViewModel
            {
                Id = t.Id,
                Fecha = t.Fecha,
                FechaAlta = t.FechaAlta,
                PacienteNombre = t.Paciente?.NombreCompleto,
                Estado = EstadoTurnoHelper.ObtenerEstado(t),
                ClaseEstado = EstadoTurnoHelper.ObtenerClaseEstado(t)
            }).ToList();

            var proximosVM = proximosTurnos.Select(t => new TurnoProfesionalViewModel
            {
                Id = t.Id,
                Fecha = t.Fecha,
                FechaAlta = t.FechaAlta,
                PacienteNombre = t.Paciente?.NombreCompleto,
                Estado = EstadoTurnoHelper.ObtenerEstado(t),
                ClaseEstado = EstadoTurnoHelper.ObtenerClaseEstado(t)
            }).ToList();

            var model = new TurnosPorDiaProfesional
            {
                TurnosDeHoy = hoyVM,
                ProximosTurnos = proximosVM
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarProximaSemana()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profesionalId = int.Parse(userId);

            DateTime hoy = DateTime.Today;
            DateTime inicioSemana = hoy.AddDays(1); // desde mañana
            DateTime finSemana = hoy.AddDays(7);    // próximos 7 días

            var turnosAConfirmar = await _context.Turnos
                .Where(t => t.ProfesionalId == profesionalId
                            && t.Fecha.Date >= inicioSemana
                            && t.Fecha.Date <= finSemana
                            && t.Confirmado == false
                            && t.Activo)
                .ToListAsync();

            foreach (var turno in turnosAConfirmar)
            {
                turno.Confirmado = true;
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Se confirmaron {turnosAConfirmar.Count} turnos de la próxima semana.";

            return RedirectToAction("ProximosTurnos");
        }
        [Authorize(Roles = $"{Config.ProfesionalRolName}")]
        public IActionResult BalanceMensual()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profesionalId = int.Parse(userId);

            var hoy = DateTime.Today;
            var primerDiaMesPasado = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-1);
            var ultimoDiaMesPasado = primerDiaMesPasado.AddMonths(1).AddDays(-1);

            var primerDiaMesActual = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDiaMesActual = primerDiaMesActual.AddMonths(1).AddDays(-1);

            var prestacion = _context.Profesionales
            .Include(p => p.Prestacion)
            .FirstOrDefault(p => p.Id == profesionalId)?.Prestacion;

            decimal valorTurno = prestacion?.Precio ?? 0;

            var turnosMesPasado = _context.Turnos
                .Where(t => t.ProfesionalId == profesionalId
                            && t.Confirmado == true
                            && t.Fecha >= primerDiaMesPasado
                            && t.Fecha <= ultimoDiaMesPasado)
                .ToList();

            var turnosMesActual = _context.Turnos
                .Where(t => t.ProfesionalId == profesionalId
                            && t.Confirmado == true
                            && t.Fecha >= primerDiaMesActual
                            && t.Fecha <= ultimoDiaMesActual) //Tenemos que ver si hacemos el calculo hasta HOY o para todo el mes.
                .ToList();

            var viewModel = new BalanceMensual
            {
                CantidadTurnos = turnosMesPasado.Count,
                ValorTurno = valorTurno,
                Total = turnosMesPasado.Count * valorTurno,
                MesCalculado = primerDiaMesPasado.ToString("MMMM yyyy"),

                TurnosMesActual = turnosMesActual.Count,
                TotalEstimadoActual = turnosMesActual.Count * valorTurno,
                MesActualNombre = primerDiaMesActual.ToString("MMMM yyyy"),

                NombrePrestacion = prestacion?.Nombre ?? "Prestación no definida"
            };


            return View(viewModel);
        }

        // GET: Turnos/CancelarTurno/5
        [HttpGet]
        [Authorize(Roles = Config.PacienteRolName)]
        public IActionResult CancelarTurno(int id)
        {
            var turno = _context.Turnos.Find(id);
            var puedeCancelar = turno != null &&
                turno.Fecha > DateTime.Now.AddHours(24);

            if (!puedeCancelar)
            {
                ViewBag.Mensaje = "No podes cancelar un turno con menos de 24hs de anticipacion";
            }

            if (turno == null || !turno.Activo)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (turno.PacienteId != int.Parse(userId))
            {
                return Forbid();
            }

            var model = new CancelarTurno
            {
                Id = turno.Id
            };

            return View(model);
        }

        // PUT: Turnos/CancelarTurno
        [Authorize(Roles = Config.PacienteRolName)]
        public IActionResult CancelarTurnoConfirm(int id, CancelarTurno model)
        {
            var turno = _context.Turnos.Find(id);
            if (turno == null || !turno.Activo)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Validación de seguridad: el turno debe pertenecer al paciente
            if (turno.PacienteId != userId)
            {
                return Forbid();
            }

            turno.Activo = false;
            turno.Confirmado = false;
            turno.DescripcionCancelacion = model.DescripcionCancelacion;

            _context.Update(turno);
            _context.SaveChanges();

            return RedirectToAction("MisTurnos");
        }

        private List<DateTime> GenerarHorariosDisponibles(Profesional profesional, double duracion)
        {
            var lista = new List<DateTime>();

            var desde = DateTime.Today;
            var hasta = desde.AddDays(7);

            for (var fecha = desde; fecha <= hasta; fecha = fecha.AddDays(1))
            {

                if (fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var inicio = profesional.HoraInicio;
                var fin = profesional.HoraFin;

                for (var slot = inicio;
                     slot.AddMinutes(duracion) <= fin;
                     slot = slot.AddMinutes(duracion))
                {
                    var slotCompleto = new DateTime(
                        fecha.Year, fecha.Month, fecha.Day,
                        slot.Hour, slot.Minute, 0
                    );

                    // Validacion para no mostrar turnos en horarios que ya pasaron
                    if (fecha.Date == DateTime.Today && slotCompleto <= DateTime.Now)
                        continue;

                    bool ocupado = profesional.Turnos != null &&
                        profesional.Turnos.Any(t => t.Fecha == slotCompleto && t.Activo);

                    if (!ocupado)
                    {
                        lista.Add(slotCompleto);
                    }
                }
            }

            return lista;
        }

        [HttpPost]
        [Authorize(Roles = $"{Config.ProfesionalRolName},{Config.AdminRolName}")]
        public async Task<IActionResult> ConfirmarTurnoIndividual(int id, string returnUrl = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.Id == id);

            if (turno == null) return NotFound();
            if (!turno.Activo) return BadRequest("El turno está inactivo.");

            if (User.IsInRole(Config.ProfesionalRolName) && turno.ProfesionalId != userId)
                return Forbid();

            turno.Confirmado = true;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return User.IsInRole(Config.AdminRolName)
                ? RedirectToAction("Index")
                : RedirectToAction("ProximosTurnos");
        }

        [HttpPost]
        [Authorize(Roles = $"{Config.ProfesionalRolName},{Config.AdminRolName}")]
        public async Task<IActionResult> CancelarTurnoIndividual(int id, string descripcionCancelacion, string returnUrl = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.Id == id);

            if (turno == null) return NotFound();

            if (!turno.Activo)
            {
                TempData["MensajeError"] = "No se puede cancelar un turno que ya fue atendido o cancelado.";

                if (!string.IsNullOrWhiteSpace(returnUrl) && !returnUrl.Contains("/Turnos/Details"))
                    return Redirect(returnUrl);

                return RedirectToAction("Details", new { id = id });
            }

            if (User.IsInRole(Config.ProfesionalRolName) && turno.ProfesionalId != userId)
                return Forbid();

            turno.Activo = false;
            turno.Confirmado = false;
            turno.DescripcionCancelacion = descripcionCancelacion;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl))
                return Redirect(returnUrl);

            return User.IsInRole(Config.AdminRolName)
                ? RedirectToAction("Index")
                : RedirectToAction("ProximosTurnos");
        }


        [HttpPost]
        [Authorize(Roles = Config.ProfesionalRolName)]
        public async Task<IActionResult> ConfirmarTurnosPorDia(DateTime fecha)
        {
            var profesionalId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var turnos = await _context.Turnos
                .Where(t => t.ProfesionalId == profesionalId && t.Fecha.Date == fecha.Date && t.Activo && !t.Confirmado)
                .ToListAsync();

            foreach (var turno in turnos)
                turno.Confirmado = true;

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = $"Se confirmaron {turnos.Count} turnos del día {fecha:dd/MM/yyyy}.";

            return RedirectToAction("ProximosTurnos");
        }

        [HttpPost]
        [Authorize(Roles = Config.ProfesionalRolName)]
        public async Task<IActionResult> CancelarTurnosPorDia(DateTime fecha, string descripcionCancelacion)
        {
            var profesionalId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var turnos = await _context.Turnos
                .Where(t => t.ProfesionalId == profesionalId && t.Fecha.Date == fecha.Date && t.Activo)
                .ToListAsync();

            foreach (var turno in turnos)
            {
                turno.Activo = false;
                turno.Confirmado = false;
                turno.DescripcionCancelacion = descripcionCancelacion ?? "Cancelado por el profesional";
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Se cancelaron {turnos.Count} turnos del día {fecha:dd/MM/yyyy}.";

            return RedirectToAction("ProximosTurnos");
        }


        [HttpPost]
        [Authorize(Roles = Config.ProfesionalRolName)]
        public async Task<IActionResult> CancelarProximaSemana(string descripcionCancelacion)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            DateTime hoy = DateTime.Today;
            DateTime inicioSemana = hoy.AddDays(1);
            DateTime finSemana = hoy.AddDays(7);

            var turnosACancelar = await _context.Turnos
                .Where(t => t.ProfesionalId == userId
                            && t.Fecha.Date >= inicioSemana
                            && t.Fecha.Date <= finSemana
                            && t.Activo)
                .ToListAsync();

            foreach (var turno in turnosACancelar)
            {
                turno.Activo = false;
                turno.Confirmado = false;
                turno.DescripcionCancelacion = descripcionCancelacion;
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Se cancelaron {turnosACancelar.Count} turnos de la próxima semana.";

            return RedirectToAction("ProximosTurnos");
        }

        [Authorize(Roles = Config.AdminRolName)]
        [HttpGet]
        public async Task<IActionResult> TurnosPorProfesional(int id, string returnUrl)
        {
            var turnos = await _context.Turnos
                .Include(t => t.Paciente)
                .Where(t => t.ProfesionalId == id)
                .ToListAsync();

            var profesional = await _context.Profesionales.FindAsync(id);

            var viewModels = turnos.Select(t => new TurnoProfesionalViewModel
            {
                Id = t.Id,
                Fecha = t.Fecha,
                FechaAlta = t.FechaAlta,
                PacienteNombre = $"{t.Paciente?.Nombre} {t.Paciente?.Apellido}",
                Estado = EstadoTurnoHelper.ObtenerEstado(t),
                ClaseEstado = EstadoTurnoHelper.ObtenerClaseEstado(t)
            }).ToList();

            ViewBag.ProfesionalNombre = $"{profesional?.Nombre} {profesional?.Apellido}";
            ViewBag.ReturnUrl = returnUrl;

            return View(viewModels);
        }


        [Authorize(Roles = Config.AdminRolName)]
        [HttpGet]
        public IActionResult SeleccionarPaciente()
        {
            var pacientes = _context.Pacientes
                .Select(p => new { p.Id, Nombre = p.NombreCompleto })
                .ToList();

            var model = new SeleccionarPacienteViewModel
            {
                Pacientes = new SelectList(pacientes, "Id", "Nombre")
            };

            return View(model);
        }

        [Authorize(Roles = Config.AdminRolName)]
        [HttpPost]
        public IActionResult SeleccionarPaciente(SeleccionarPacienteViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var pacienteId = model.PacienteId.Value;

            var tieneTurnoActivo = _context.Turnos.Any(t => t.PacienteId == pacienteId && t.Activo);
            if (tieneTurnoActivo)
            {
                TempData["MensajeError"] = "El paciente ya tiene un turno activo. No se puede agendar otro.";
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetInt32("PacienteId", pacienteId);
            return RedirectToAction("SeleccionarPrestacion");
        }


    }
}
