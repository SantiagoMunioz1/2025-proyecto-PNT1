using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel;

namespace Turnos.C.Models.ViewModels
{
    public class SeleccionarProfesional
    {

        public SelectList Profesionales { get; set; }

        [DisplayName("Profesional")]
        public int? ProfesionalId { get; set; }
    }
}
