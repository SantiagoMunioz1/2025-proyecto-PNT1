using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.ComponentModel.DataAnnotations;
using Turnos.C.Helpers;

namespace Turnos.C.Models
{
    public class Turno
    {
        public int Id { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public DateTime Fecha { get; set; }

        public bool Confirmado { get; set; } = true;

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public bool Activo { get; set; } = true;

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public DateTime FechaAlta { get; set; } = DateTime.Now;

        // Propiedad navegacional
        public Paciente Paciente { get; set; }

        //Propiedad relacional
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [Display(Name = "Apellido del paciente")]
        public int PacienteId { get; set; }

        //Propiedad navegacional
        public Profesional Profesional { get; set; }

        //Propiedad relacional
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [Display(Name ="Apellido del profesional")]
        public int ProfesionalId { get; set; }
        
        [DataType(DataType.MultilineText)]
        [StringLength(
            500,
            MinimumLength = 10,
            ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression("^[\\sa-zA-Z0-9_ñÑ-]*$", ErrorMessage = MsjsError.ERROR_CARACTERES_INVALIDOS)]

        public string? DescripcionCancelacion { get; set; }
    }
}
