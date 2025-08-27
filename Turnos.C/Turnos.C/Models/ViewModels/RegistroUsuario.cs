using System.ComponentModel.DataAnnotations;
using Turnos.C.Helpers;

namespace Turnos.C.Models.ViewModels
{
    public class RegistroUsuario
    {
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Por favor ingresa un email valido")]
        public string Email { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }


        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmacion Contraseña")]
        [Compare(
            "Password",
            ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden."
            )]
        public string ConfirmacionPassword { get; set; }

    }
}
