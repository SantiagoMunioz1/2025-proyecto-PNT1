using System.ComponentModel.DataAnnotations;
using Turnos.C.Helpers;

namespace Turnos.C.Models
{
    public class Cobertura
    {
        public int Id { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength(
            12, 
            MinimumLength = 6, 
            ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression("^[0-9]*$", ErrorMessage = MsjsError.ERROR_SOLO_NUMEROS)]
        public string NumeroCredencial { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public Prestadora Prestadora { get; set; } 
        
        public Paciente Paciente { get; set; } // Propiedad Navegacional

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public int PacienteId { get; set; } // Propiedad Relacional
    }
}
