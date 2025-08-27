using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Turnos.C.Helpers;
using Turnos.C.Models;
using Turnos.C.Data;
using System.Collections.Generic;
using System.Linq;

namespace Turnos.C.Models.ViewModels
{
    public class PacienteViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [EmailAddress(ErrorMessage = "Por favor ingresa un email valido")]
        public string Email { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength(30, MinimumLength = 2, ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\\s'\\-]*$", ErrorMessage = MsjsError.ERROR_SOLO_LETRAS)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength(30, MinimumLength = 2, ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\\s'\\-]*$", ErrorMessage = MsjsError.ERROR_SOLO_LETRAS)]
        public string Apellido { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [RegularExpression(@"^[0-9]{7,9}$", ErrorMessage = "Por favor ingresa un DNI valido")]
        [StringLength(9, MinimumLength = 7, ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        public string DNI { get; set; }

        public int? CoberturaId { get; set; }
        public List<SelectListItem> Coberturas { get; set; }

        [StringLength(12, MinimumLength = 6, ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression("^[0-9]*$", ErrorMessage = MsjsError.ERROR_SOLO_NUMEROS)]
        [DisplayName("Número de Credencial")]
        public string NuevaCoberturaNumeroCredencial { get; set; }

        [DisplayName("Prestadora")]
        public Prestadora? NuevaCoberturaPrestadora { get; set; }

        public List<TelefonoViewModel> Telefonos { get; set; } = new();

        public DireccionViewModel Direccion { get; set; } = new();

        // ✅ Validación de número de credencial duplicado
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var context = (TurnosContext)validationContext.GetService(typeof(TurnosContext));

            if (!string.IsNullOrWhiteSpace(NuevaCoberturaNumeroCredencial))
            {
                bool existeDuplicado = context.Coberturas
                    .Any(c => c.NumeroCredencial == NuevaCoberturaNumeroCredencial && c.PacienteId != Id);

                if (existeDuplicado)
                {
                    yield return new ValidationResult(
                        "Ese número de credencial ya está en uso.",
                        new[] { nameof(NuevaCoberturaNumeroCredencial) });
                }
            }

            if (!string.IsNullOrWhiteSpace(DNI))
            {
                bool dniDuplicado = context.Personas
                    .Any(p => p.DNI == DNI && p.Id != Id);

                if (dniDuplicado)
                {
                    yield return new ValidationResult(
                        "Ese DNI ya está en uso.",
                        new[] { nameof(DNI) });
                }
            }
        }
    }
}
