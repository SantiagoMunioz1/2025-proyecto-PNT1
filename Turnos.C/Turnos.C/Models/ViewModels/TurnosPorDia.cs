using System.Collections.Generic;

namespace Turnos.C.Models.ViewModels
{
    public class TurnosPorDia
    {
        public List<Turno> TurnosDeHoy { get; set; }
        public List<Turno> ProximosTurnos { get; set; }
    }
}
