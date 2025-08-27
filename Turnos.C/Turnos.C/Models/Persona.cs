using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Turnos.C.Helpers;

namespace Turnos.C.Models
{
    public class Persona : IdentityUser<int>
    {

        //[Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        //[StringLength(
        //    20,
        //    MinimumLength = 3,
        //    ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        //public string UserName { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Por favor ingresa un email valido")]
        public override string Email {
            get { return base.Email; }
            set { base.Email = value; } 
        }
        
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public DateTime FechaAlta { get; set; } = DateTime.Now;

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength(
            30,
            MinimumLength = 2,
            ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\\s'\\-]*$", ErrorMessage = MsjsError.ERROR_SOLO_LETRAS)]

        public string Nombre { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength(
            30,
            MinimumLength = 2,
            ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\\s'\\-]*$", ErrorMessage = MsjsError.ERROR_SOLO_LETRAS)]

        public string Apellido { get; set; }

        public string NombreCompleto => $"{Nombre.Trim()} {Apellido.Trim()}";

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [RegularExpression(@"^[0-9]{7,9}$", ErrorMessage = "Por favor ingresa un DNI valido")]
        [StringLength(
            9,
            MinimumLength = 7,
            ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        public string DNI { get; set; }

        public List<Telefono> Telefonos { get; set; }
        public Direccion Direccion { get; set; }
    }
}