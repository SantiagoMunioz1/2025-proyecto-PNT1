using System;
using Turnos.C.Models;

namespace Turnos.C.Models.ViewModels
{
    public class TurnoPacienteViewModel
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string PrestacionNombre { get; set; }
        public string ProfesionalNombre { get; set; }
        public string Estado { get; set; }
        public string ClaseEstado { get; set; }
        public bool Confirmado { get; set; }
        public string DescripcionCancelacion { get; set; }
        public bool PuedeCancelar { get; set; }
    }
}
