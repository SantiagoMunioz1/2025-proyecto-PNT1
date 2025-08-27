using System.ComponentModel.DataAnnotations;
using Turnos.C.Helpers;

namespace Turnos.C.Models
{

    public class Matricula
    {

        public int Id { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength(10, MinimumLength = 2, ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression("^[0-9]*$", ErrorMessage = MsjsError.ERROR_SOLO_NUMEROS)]
        public string NumeroMatricula { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public Provincia Provincia { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public TipoMatricula Tipo { get; set; }

        //prop navegacional
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public int ProfesionalId { get; set; }

        public Profesional Profesional { get; set; }

    }

}
