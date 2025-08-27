using System.Collections.Generic;

namespace Turnos.C.Models.ViewModels
{
    public class TurnosPorDiaProfesional
    {
        public List<TurnoProfesionalViewModel> TurnosDeHoy { get; set; }
        public List<TurnoProfesionalViewModel> ProximosTurnos { get; set; }
    }
}