namespace AssesmentTecnico.Models
{
    public enum Prioridad
    {
        Baja,
        Media,
        Alta
    }

    public enum EstadoRecordatorio
    {
        Pendiente,
        Notificado,
        Resuelto,
        Vencido
    }

public enum TipoAlerta
{
    ProximoAVencer,
    Critico
}
// ENUMS = TIPOS DE DATOS ESTRICTOS => MENOS PROBABILIDAD ERRORES

    public class Recordatorio
    {
        public int Id { get; set; }
        public string TipoVencimiento { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int ConsorcioId { get; set; }
        public EstadoRecordatorio Estado { get; set; }
        public Prioridad Prioridad { get; set; }
    }
}
