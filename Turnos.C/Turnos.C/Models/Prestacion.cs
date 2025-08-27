using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Turnos.C.Helpers;

namespace Turnos.C.Models
{
    public class Prestacion
    {

        public int Id { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength(
            20, 
            MinimumLength = 2, 
            ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression("^[a-zA-ZñÑáéíóúÁÉÍÓÚ \\-']+$", ErrorMessage = MsjsError.ERROR_SOLO_LETRAS)]
        public string Nombre { get; set; }
        
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength(
            200,
            MinimumLength = 10,
            ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [DataType(DataType.MultilineText)]
        [RegularExpression("^[a-zA-Z0-9_ñÑáéíóúÁÉÍÓÚ\\- .,()/'\":]*$", ErrorMessage = MsjsError.ERROR_CARACTERES_INVALIDOS)]

        public string Descripcion { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [Range(1, 180, ErrorMessage = MsjsError.ERROR_RANGO)]
        [Display(Name = "Duración en minutos")]     
        public int Duracion { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [Range(1, 1000000, ErrorMessage = MsjsError.ERROR_RANGO)]
        public decimal Precio { get; set; } // en este caso creo que podemos usar decimal en vez de double
                                           //required
        public List<Profesional> Profesionales { get; set; } // Podemos instanciar la lista directamente
                                                             // haciendo un = new List<Profesional>() para
                                                             // evitar NullReferenceException en el futuro

    }
}
