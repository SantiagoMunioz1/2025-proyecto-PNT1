using System.ComponentModel.DataAnnotations;
using Turnos.C.Helpers;
using Turnos.C.Models;

namespace Turnos.C.Models.ViewModels
{
    public class TelefonoViewModel
    {
        public int? Id { get; set; }

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
    }
}
