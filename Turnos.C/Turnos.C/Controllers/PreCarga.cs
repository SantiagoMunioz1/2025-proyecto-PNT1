
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Turnos.C.Data;
using Turnos.C.Helpers;
using Turnos.C.Models;

namespace Turnos.C.Controllers
{
    public partial class PreCarga : Controller
    {
        private readonly UserManager<Persona> _userManager;
        private readonly RoleManager<Rol> _roleManager;
        private readonly TurnosContext _context;

        private List<string> roles = new List<string>()
        {
            Config.AdminRolName,
            Config.PacienteRolName,
            Config.ProfesionalRolName,
            Config.UsuarioRolName
        };

        public PreCarga(UserManager<Persona> userManager, RoleManager<Rol> roleManager, TurnosContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public IActionResult Seed()
        {
            _context.Database.EnsureDeleted();
            _context.Database.Migrate();
            CrearSeedCompleto().Wait();
            return RedirectToAction("Index", "Home", new { mensaje = "Proceso Seed Finalizado" });
        }

        private async Task CrearSeedCompleto()
        {
            await CrearRoles();
            await CrearAdmin();
            await CrearPrestaciones();
            await CrearProfesionales();
            var pacientes = await CrearPacientes();
            await CrearTurnos(pacientes);
            await DesactivarTurnosPasados();
        }

        private async Task CrearRoles()
        {
            foreach (var rolName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(rolName))
                {
                    await _roleManager.CreateAsync(new Rol(rolName));
                }
            }
        }

        private async Task CrearAdmin()
        {
            const string adminEmail = "admin@ort.edu.ar";
            const string adminDNI = "99999999";

            var adminExistente = await _userManager.FindByEmailAsync(adminEmail);
            if (adminExistente != null) return;

            var admin = new Persona
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nombre = "Admin",
                Apellido = "Sistema",
                DNI = adminDNI
            };

            var result = await _userManager.CreateAsync(admin, Config.PasswordGenerica);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, Config.AdminRolName);
            }
        }

        private async Task CrearPrestaciones()
        {
            var prestaciones = new List<Prestacion>
            {
                new Prestacion { Nombre = "Kinesiología", Descripcion = "Terapias físicas", Duracion = 60, Precio = 3000 },
                new Prestacion { Nombre = "Cardiología", Descripcion = "Enfermedades del corazón", Duracion = 45, Precio = 5000 },
                new Prestacion { Nombre = "Dermatología", Descripcion = "Salud de la piel", Duracion = 30, Precio = 4000 },
                new Prestacion { Nombre = "Pediatría", Descripcion = "Medicina infantil", Duracion = 30, Precio = 3500 },
                new Prestacion { Nombre = "Ginecología", Descripcion = "Sistema reproductor femenino", Duracion = 60, Precio = 4500 },
                new Prestacion { Nombre = "Neurología", Descripcion = "Sistema nervioso", Duracion = 60, Precio = 6000 },
                new Prestacion { Nombre = "Oftalmología", Descripcion = "Vista y ojos", Duracion = 40, Precio = 4800 }
            };

            foreach (var prestacion in prestaciones)
            {
                if (!await _context.Prestaciones.AnyAsync(p => p.Nombre == prestacion.Nombre))
                    await _context.Prestaciones.AddAsync(prestacion);
            }

            await _context.SaveChangesAsync();
        }

        private async Task CrearProfesionales()
        {
            var prestaciones = await _context.Prestaciones.ToDictionaryAsync(p => p.Nombre, p => p);

            var profesionales = new List<Profesional>
            {
                new Profesional { UserName = "profesional1@ort.edu.ar", Email = "profesional1@ort.edu.ar", Nombre = "Laura", Apellido = "Mendoza", DNI = "20111222", PrestacionId = prestaciones["Kinesiología"].Id, HoraInicio = new TimeOnly(9,0), HoraFin = new TimeOnly(16,0), Matricula = new Matricula { NumeroMatricula = "10001", Provincia = Provincia.BuenosAires, Tipo = TipoMatricula.Nacional } },
                new Profesional { UserName = "profesional2@ort.edu.ar", Email = "profesional2@ort.edu.ar", Nombre = "Federico", Apellido = "Luna", DNI = "20222333", PrestacionId = prestaciones["Cardiología"].Id, HoraInicio = new TimeOnly(8,0), HoraFin = new TimeOnly(14,0), Matricula = new Matricula { NumeroMatricula = "10002", Provincia = Provincia.Cordoba, Tipo = TipoMatricula.Nacional } },
                new Profesional { UserName = "profesional3@ort.edu.ar", Email = "profesional3@ort.edu.ar", Nombre = "Julia", Apellido = "Santos", DNI = "20333444", PrestacionId = prestaciones["Dermatología"].Id, HoraInicio = new TimeOnly(10,0), HoraFin = new TimeOnly(17,0), Matricula = new Matricula { NumeroMatricula = "10003", Provincia = Provincia.Salta, Tipo = TipoMatricula.Provincial } },
                new Profesional { UserName = "profesional4@ort.edu.ar", Email = "profesional4@ort.edu.ar", Nombre = "Hernan", Apellido = "Delgado", DNI = "20444555", PrestacionId = prestaciones["Pediatría"].Id, HoraInicio = new TimeOnly(9,30), HoraFin = new TimeOnly(15,30), Matricula = new Matricula { NumeroMatricula = "10004", Provincia = Provincia.Chubut, Tipo = TipoMatricula.Nacional } },
                new Profesional { UserName = "profesional5@ort.edu.ar", Email = "profesional5@ort.edu.ar", Nombre = "Cecilia", Apellido = "Nuñez", DNI = "20555666", PrestacionId = prestaciones["Ginecología"].Id, HoraInicio = new TimeOnly(8,0), HoraFin = new TimeOnly(13,0), Matricula = new Matricula { NumeroMatricula = "10005", Provincia = Provincia.Corrientes, Tipo = TipoMatricula.Provincial } },
                new Profesional { UserName = "profesional6@ort.edu.ar", Email = "profesional6@ort.edu.ar", Nombre = "Ignacio", Apellido = "Rey", DNI = "20666777", PrestacionId = prestaciones["Neurología"].Id, HoraInicio = new TimeOnly(10,0), HoraFin = new TimeOnly(16,0), Matricula = new Matricula { NumeroMatricula = "10006", Provincia = Provincia.SanLuis, Tipo = TipoMatricula.Nacional } },
                new Profesional { UserName = "profesional7@ort.edu.ar", Email = "profesional7@ort.edu.ar", Nombre = "Julieta", Apellido = "Paredes", DNI = "20777888", PrestacionId = prestaciones["Oftalmología"].Id, HoraInicio = new TimeOnly(9,0), HoraFin = new TimeOnly(15,0), Matricula = new Matricula { NumeroMatricula = "10007", Provincia = Provincia.LaPampa, Tipo = TipoMatricula.Nacional } },
                new Profesional { UserName = "profesional8@ort.edu.ar", Email = "profesional8@ort.edu.ar", Nombre = "Esteban", Apellido = "Gallardo", DNI = "20888999", PrestacionId = prestaciones["Kinesiología"].Id, HoraInicio = new TimeOnly(7,0), HoraFin = new TimeOnly(14,0), Matricula = new Matricula { NumeroMatricula = "10008", Provincia = Provincia.Formosa, Tipo = TipoMatricula.Provincial } },
                new Profesional { UserName = "profesional9@ort.edu.ar", Email = "profesional9@ort.edu.ar", Nombre = "Natalia", Apellido = "Acosta", DNI = "20999000", PrestacionId = prestaciones["Dermatología"].Id, HoraInicio = new TimeOnly(9,0), HoraFin = new TimeOnly(17,0), Matricula = new Matricula { NumeroMatricula = "10009", Provincia = Provincia.Jujuy, Tipo = TipoMatricula.Nacional } },
                new Profesional { UserName = "profesional10@ort.edu.ar", Email = "profesional10@ort.edu.ar", Nombre = "Matias", Apellido = "Fernandez", DNI = "21000111", PrestacionId = prestaciones["Pediatría"].Id, HoraInicio = new TimeOnly(10,0), HoraFin = new TimeOnly(18,0), Matricula = new Matricula { NumeroMatricula = "10010", Provincia = Provincia.SantaCruz, Tipo = TipoMatricula.Provincial } }
            };

            foreach (var profesional in profesionales)
            {
                var existente = await _userManager.FindByEmailAsync(profesional.Email);
                if (existente == null)
                {
                    var result = await _userManager.CreateAsync(profesional, Config.PasswordGenerica);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(profesional, Config.ProfesionalRolName);
                    }
                }
            }
        }

        private async Task<List<Paciente>> CrearPacientes()
        {
            var pacientes = new List<Paciente>
        {
            new Paciente { UserName = "paciente1@ort.edu.ar", Email = "paciente1@ort.edu.ar", Nombre = "Juan", Apellido = "Perez", DNI = "30111222" },
            new Paciente { UserName = "paciente2@ort.edu.ar", Email = "paciente2@ort.edu.ar", Nombre = "Ana", Apellido = "Gomez", DNI = "30222333" },
            new Paciente { UserName = "paciente3@ort.edu.ar", Email = "paciente3@ort.edu.ar", Nombre = "Carlos", Apellido = "Lopez", DNI = "30333444" },
            new Paciente { UserName = "paciente4@ort.edu.ar", Email = "paciente4@ort.edu.ar", Nombre = "Lucia", Apellido = "Martinez", DNI = "30444555" },
            new Paciente { UserName = "paciente5@ort.edu.ar", Email = "paciente5@ort.edu.ar", Nombre = "Sofia", Apellido = "Fernandez", DNI = "30555666" },
            new Paciente { UserName = "paciente6@ort.edu.ar", Email = "paciente6@ort.edu.ar", Nombre = "Mariano", Apellido = "Alonso", DNI = "30666777" },
            new Paciente { UserName = "paciente7@ort.edu.ar", Email = "paciente7@ort.edu.ar", Nombre = "Paula", Apellido = "Dominguez", DNI = "30777888" },
            new Paciente { UserName = "paciente8@ort.edu.ar", Email = "paciente8@ort.edu.ar", Nombre = "Diego", Apellido = "Suarez", DNI = "30888999" },
            new Paciente { UserName = "paciente9@ort.edu.ar", Email = "paciente9@ort.edu.ar", Nombre = "Camila", Apellido = "Blanco", DNI = "30999000" },
            new Paciente { UserName = "paciente10@ort.edu.ar", Email = "paciente10@ort.edu.ar", Nombre = "Leonardo", Apellido = "Rossi", DNI = "31000111" },
            new Paciente { UserName = "paciente11@ort.edu.ar", Email = "paciente11@ort.edu.ar", Nombre = "Martina", Apellido = "Bello", DNI = "31111222" },
            new Paciente { UserName = "paciente12@ort.edu.ar", Email = "paciente12@ort.edu.ar", Nombre = "Emanuel", Apellido = "Silva", DNI = "31222333" },
            new Paciente { UserName = "paciente13@ort.edu.ar", Email = "paciente13@ort.edu.ar", Nombre = "Valeria", Apellido = "Ortega", DNI = "31333444" },
            new Paciente { UserName = "paciente14@ort.edu.ar", Email = "paciente14@ort.edu.ar", Nombre = "Hugo", Apellido = "Soria", DNI = "31444555" },
            new Paciente { UserName = "paciente15@ort.edu.ar", Email = "paciente15@ort.edu.ar", Nombre = "Micaela", Apellido = "Torres", DNI = "31555666" }
        };


            var pacientesCreados = new List<Paciente>();
            foreach (var paciente in pacientes)
            {
                var existente = await _userManager.FindByEmailAsync(paciente.Email);
                if (existente == null)
                {
                    var result = await _userManager.CreateAsync(paciente, Config.PasswordGenerica);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(paciente, Config.PacienteRolName);
                        pacientesCreados.Add(paciente);
                    }
                }
                else if (existente is Paciente yaExistente)
                {
                    pacientesCreados.Add(yaExistente);
                }
            }
            return pacientesCreados;
        }
        private async Task CrearTurnos(List<Paciente> pacientes)
        {
            // Traemos todos los profesionales junto con su Prestación para acceder a la duración
            var profesionales = await _context.Profesionales
                .Include(p => p.Prestacion)
                .ToListAsync();

            // Definimos tres fechas de referencia:
            // - Una en el pasado para simular turnos ya tomados (no activos)
            // - Dos en el futuro para simular turnos disponibles
            var fechaPasada = DateTime.Today.AddDays(-2);
            var fechaFutura1 = DateTime.Today.AddDays(1);
            var fechaFutura2 = DateTime.Today.AddDays(6);

            // Lista donde se acumularán los nuevos turnos
            var turnos = new List<Turno>();

            // Usamos una cola para asignar pacientes secuencialmente a los turnos
            var pacientesDisponibles = new Queue<Paciente>(pacientes.OrderBy(p => p.Id).ToList());

            // Iteramos por cada profesional
            foreach (var profesional in profesionales)
            {
                // Calculamos la duración de la prestación del profesional
                var duracionMinutos = profesional.Prestacion?.Duracion;
                var duracion = TimeSpan.FromMinutes((double)duracionMinutos);

                // Definimos tres horarios consecutivos basados en su hora de inicio
                var hora1 = profesional.HoraInicio.ToTimeSpan();
                var hora2 = hora1.Add(duracion);
                var hora3 = hora2.Add(duracion);

                // Creamos un turno en el pasado (simula historial ya registrado)
                // El turno está confirmado y no activo
                if (pacientesDisponibles.TryDequeue(out var paciente1))
                {
                    turnos.Add(new Turno
                    {
                        ProfesionalId = profesional.Id,
                        PacienteId = paciente1.Id,
                        Fecha = fechaPasada.Date.Add(hora1),
                        Activo = false,
                        Confirmado = true,
                        FechaAlta = fechaPasada.AddDays(-1) // se creó incluso antes de la fecha del turno
                    });
                }

                // Creamos un turno futuro no confirmado (simula reserva en espera)
                if (pacientesDisponibles.TryDequeue(out var paciente2))
                {
                    turnos.Add(new Turno
                    {
                        ProfesionalId = profesional.Id,
                        PacienteId = paciente2.Id,
                        Fecha = fechaFutura1.Date.Add(hora2),
                        Activo = true,
                        Confirmado = false,
                        FechaAlta = DateTime.Now
                    });
                }

                // Creamos otro turno futuro más lejano (simula plan de agenda)
                if (pacientesDisponibles.TryDequeue(out var paciente3))
                {
                    turnos.Add(new Turno
                    {
                        ProfesionalId = profesional.Id,
                        PacienteId = paciente3.Id,
                        Fecha = fechaFutura2.Date.Add(hora3),
                        Activo = true,
                        Confirmado = false,
                        FechaAlta = DateTime.Now
                    });
                }
            }

            // Guardado en db
            await _context.Turnos.AddRangeAsync(turnos);
            await _context.SaveChangesAsync();
        }

        // Helper para desarrollo
        public async Task<IActionResult> DesactivarTurnosPasados()
        {
            var ahora = DateTime.Now;

            var turnosPasados = await _context.Turnos
                .Where(t => t.Fecha < ahora && t.Activo)
                .ToListAsync();

            foreach (var turno in turnosPasados)
            {
                turno.Activo = false;
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Se desactivaron {turnosPasados.Count} turnos pasados";
            return RedirectToAction("Index", "Turnos");
        }

    }
}
