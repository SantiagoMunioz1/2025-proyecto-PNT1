using System.ComponentModel.DataAnnotations;
using Turnos.C.Helpers;

namespace Turnos.C.Models
{
    public class Telefono
    {
        public int Id { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength(5, MinimumLength = 1, ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression(@"^\d+$", ErrorMessage = MsjsError.ERROR_SOLO_NUMEROS)]
        public string Prefijo { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength(10, MinimumLength = 6, ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression(@"^\d+$", ErrorMessage = MsjsError.ERROR_SOLO_NUMEROS)]
        public string Numero { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public TipoTelefono TipoTelefono { get; set; }

        public Persona Persona { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public int PersonaId { get; set; }
    }

    public enum TipoTelefono
    {
        Fijo,
        Celular,
        Otro
    }
}
