namespace AssesmentTecnico.Models
{
    public class PrioridadAsignada
    {
        public int Id { get; set; }
        public Prioridad Prioridad { get; set; }
    }

    public class ResultadoIa
    {
        public string Resumen { get; set; } = string.Empty;
        public List<PrioridadAsignada> Prioridades { get; set; } = new();
    }
}
