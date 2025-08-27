using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Turnos.C.Data;

namespace Turnos.C.Models.ViewModels
{
    public class GenericUserViewModel : IValidatableObject
    {

        public int Id { get; set; }
        [Required]
        public string Role { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 2)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 2)]
        public string Apellido { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{7,9}$")]
        public string DNI { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var context = (TurnosContext)validationContext.GetService(typeof(TurnosContext));

            if (!string.IsNullOrWhiteSpace(DNI))
            {
                bool dniDuplicado = context.Personas.Any(p =>
                    p.DNI == DNI && p.Id != Id); // excluir el actual
                if (dniDuplicado)
                    yield return new ValidationResult("Ese DNI ya está en uso.", new[] { nameof(DNI) });
            }

            if (!string.IsNullOrWhiteSpace(Email))
            {
                bool emailDuplicado = context.Personas.Any(p =>
                    p.Email.ToLower() == Email.ToLower() && p.Id != Id); // excluir el actual
                if (emailDuplicado)
                    yield return new ValidationResult("Ese Email ya está en uso.", new[] { nameof(Email) });
            }
        }

    }
}