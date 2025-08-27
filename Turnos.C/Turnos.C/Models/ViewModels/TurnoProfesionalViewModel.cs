using System;

namespace Turnos.C.Models.ViewModels
{
    public class TurnoProfesionalViewModel
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaAlta { get; set; }
        public string PacienteNombre { get; set; }
        public string Estado { get; set; }
        public string ClaseEstado { get; set; }
    }
}