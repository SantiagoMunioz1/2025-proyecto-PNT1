namespace Turnos.C.Models.ViewModels
{
    public class CancelarTurno
    {
        public int Id { get; set; }
        public bool PuedeCancelar { get; set; }
        public string DescripcionCancelacion { get; set; } = string.Empty;
    }
}
