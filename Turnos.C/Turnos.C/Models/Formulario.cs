using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Turnos.C.Helpers;

namespace Turnos.C.Models
{
    public class Formulario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [EmailAddress(ErrorMessage = "Por favor ingresa un email valido")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }
        
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength(
            30, 
            MinimumLength = 2, 
            ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\\s'\\-]*$", ErrorMessage = MsjsError.ERROR_SOLO_LETRAS)]
        public string Nombre { get; set; }
        
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength
            (30, 
            MinimumLength = 2, 
            ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\\s'\\-]*$", ErrorMessage = MsjsError.ERROR_SOLO_LETRAS)]
        public string Apellido { get; set; }
        
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [RegularExpression(@"^[0-9]{7,9}$", ErrorMessage = "Por favor ingresa un DNI valido")]
        public string DNI { get; set; }

        public bool Leido { get; set; } = false;
        
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public string Titulo { get; set; }
        
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [MaxLength(60, ErrorMessage = "El campo {0} no debe superar los {1} caracteres")]
        [MinLength(2, ErrorMessage = "El campo {0} debe superar los {1} caracteres")]
        public string Mensaje { get; set; }

        [ForeignKey("Paciente")]
        public int? PacienteId { get; set; }
        
        public Paciente? Paciente { get; set; }
    }
}

