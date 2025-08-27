using System.ComponentModel.DataAnnotations;
using Turnos.C.Helpers;

namespace Turnos.C.Models.ViewModels
{
    public class DireccionViewModel
    {
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [Display(Name = "Calle")]
        public string Calle { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [Range(1, int.MaxValue, ErrorMessage = MsjsError.ERROR_RANGO)]
        [Display(Name = "Altura")]
        public int Altura { get; set; }

        [Range(0, 100, ErrorMessage = MsjsError.ERROR_RANGO)]
        [Display(Name = "Piso")]
        public int? Piso { get; set; }

        [StringLength(10, MinimumLength = 1, ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [Display(Name = "Departamento")]
        public string? Departamento { get; set; }
    }
}