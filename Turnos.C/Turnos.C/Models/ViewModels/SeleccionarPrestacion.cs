using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel;

namespace Turnos.C.Models.ViewModels
{
    public class SeleccionarPrestacion
    {

        public SelectList Prestaciones { get; set; }

        [DisplayName("Prestacion")]
        public int? PrestacionId { get; set; }
    }
}
