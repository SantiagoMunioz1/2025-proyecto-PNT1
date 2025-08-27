using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Turnos.C.Data;
using Turnos.C.Helpers;
using System.Linq;

namespace Turnos.C.Models.ViewModels
{
    public class ProfesionalViewModel : IValidatableObject
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

        public int? PrestacionId { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [DataType(DataType.Time)]
        [DisplayName("Hora Inicio")]
        public TimeOnly HoraInicio { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [DataType(DataType.Time)]
        [DisplayName("Hora Finalizacion")]
        public TimeOnly HoraFin { get; set; }

        public List<SelectListItem> Prestaciones { get; set; }

        // Nueva prestación
        [StringLength(20, MinimumLength = 2, ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\\s'\\-]*$", ErrorMessage = MsjsError.ERROR_SOLO_LETRAS)]
        [DisplayName("Nombre Prestacion")]
        public string NuevaPrestacionNombre { get; set; }

        [StringLength(200, MinimumLength = 10, ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [DataType(DataType.MultilineText)]
        [RegularExpression("^[a-zA-Z0-9_ñÑáéíóúÁÉÍÓÚ\\- .,()/'\":]*$", ErrorMessage = MsjsError.ERROR_CARACTERES_INVALIDOS)]
        [DisplayName("Descripcion Prestacion")]
        public string NuevaPrestacionDescripcion { get; set; }

        [Range(1, 180, ErrorMessage = MsjsError.ERROR_RANGO)]
        [DisplayName("Duracion (en minutos)")]
        public int? NuevaPrestacionDuracion { get; set; }

        [Range(1, 1000000, ErrorMessage = MsjsError.ERROR_RANGO)]
        [DisplayName("Precio")]
        public decimal? NuevaPrestacionPrecio { get; set; }

        // Matrícula
        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        [StringLength(10, MinimumLength = 2, ErrorMessage = MsjsError.ERROR_STRING_LENGTH)]
        [RegularExpression("^[0-9]*$", ErrorMessage = MsjsError.ERROR_SOLO_NUMEROS)]
        [DisplayName("Numero de Matricula")]
        public string NumeroMatricula { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public Provincia Provincia { get; set; }

        [Required(ErrorMessage = MsjsError.ERROR_REQUERIDO)]
        public TipoMatricula Tipo { get; set; }

        public List<TelefonoViewModel> Telefonos { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var context = (TurnosContext)validationContext.GetService(typeof(TurnosContext));

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

            if (!string.IsNullOrWhiteSpace(NumeroMatricula))
            {
                bool matriculaDuplicada = context.Matriculas
                    .Any(m => m.NumeroMatricula == NumeroMatricula && m.ProfesionalId != Id);

                if (matriculaDuplicada)
                {
                    yield return new ValidationResult(
                        "Ese número de matrícula ya está en uso.",
                        new[] { nameof(NumeroMatricula) });
                }
            }

            if (!string.IsNullOrWhiteSpace(NuevaPrestacionNombre))
            {
                bool yaExiste = context.Prestaciones
                    .Any(p => p.Nombre.ToLower() == NuevaPrestacionNombre.ToLower());

                if (yaExiste)
                {
                    yield return new ValidationResult(
                        "Ya existe una prestación con ese nombre.",
                        new[] { nameof(NuevaPrestacionNombre) });
                }
            }
        }
    }
}
