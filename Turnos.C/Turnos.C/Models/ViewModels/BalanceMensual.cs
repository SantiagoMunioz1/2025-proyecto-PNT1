public class BalanceMensual
{
    public int CantidadTurnos { get; set; }
    public decimal ValorTurno { get; set; }
    public decimal Total { get; set; }
    public string MesCalculado { get; set; }

    public int TurnosMesActual { get; set; }
    public decimal TotalEstimadoActual { get; set; }
    public string MesActualNombre { get; set; }

    public string NombrePrestacion { get; set; }
}