using Turnos.C.Models;

namespace Turnos.C.Helpers
{
    public static class EstadoTurnoHelper
    {
        public static string ObtenerEstado(Turno turno)
        {
            return (turno.Activo, turno.Confirmado) switch
            {
                (true, true) => "Confirmado",
                (true, false) => "Pendiente de confirmación",
                (false, true) => "Finalizado",
                (false, false) => "Cancelado",
            };
        }

        public static string ObtenerClaseEstado(Turno turno)
        {
            return (turno.Activo, turno.Confirmado) switch
            {
                (true, true) => "badge bg-success",                  // verde
                (true, false) => "badge bg-warning text-dark",       // amarillo
                (false, true) => "badge bg-secondary",               // gris
                (false, false) => "badge bg-danger",                 // rojo
            };
        }
    }
}
