using System;
using System.Collections.Generic;

namespace Turnos.C.Models.ViewModels
{
    public class SeleccionarHorario
    {
        public List<DateTime> HorariosDisponibles { get; set; }
        public DateTime SelectedHorario { get; set; }
    }
}
