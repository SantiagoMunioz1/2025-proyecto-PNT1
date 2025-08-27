using System.ComponentModel.DataAnnotations;
using Turnos.C.Helpers;

namespace Turnos.C.Models
{
    public class Direccion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength(
            50, 
            MinimumLength = 1, 
            ErrorMessage = MsjsError.ERROR_STRING_LENGTH
            )]
        public string Calle { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [Range(
            1, 
            int.MaxValue, 
            ErrorMessage = MsjsError.ERROR_RANGO
            )]
        public int Altura { get; set; }

        [Range(0, 100, ErrorMessage = MsjsError.ERROR_RANGO)]
        public int? Piso { get; set; }
        [StringLength(
            10, 
            MinimumLength = 1, 
            ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        public string? Departamento { get; set; }

        public Persona Persona { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public int PersonaId { get; set; }
    }
}