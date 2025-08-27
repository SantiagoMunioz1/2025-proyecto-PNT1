using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Turnos.C.Models.ViewModels
{
    public class SeleccionarPacienteViewModel
    {
        [Required]
        [Display(Name = "Paciente")]
        public int? PacienteId { get; set; }

        public SelectList Pacientes { get; set; }
    }
}
