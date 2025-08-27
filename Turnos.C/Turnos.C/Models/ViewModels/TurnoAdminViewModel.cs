using System;

namespace Turnos.C.Models.ViewModels
{
    public class TurnoAdminViewModel
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string PacienteNombre { get; set; }
        public string ProfesionalNombre { get; set; }
        public string Estado { get; set; }
        public string ClaseEstado { get; set; }
    }

}
