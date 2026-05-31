using AssesmentTecnico.Models;

namespace AssesmentTecnico.Services
{
    public class RecordatorioService
    {
        private readonly List<Recordatorio> _recordatorios;

        public RecordatorioService(List<Recordatorio> recordatorios)
        {
            _recordatorios = recordatorios;
        }

        public List<Recordatorio> ObtenerVencidos() // Vencidos ordenados por antiguedad
        {
            return _recordatorios
                .Where(r => r.FechaVencimiento < DateTime.Now)
                .OrderBy(r => r.FechaVencimiento)
                .ToList();
        }

        public List<Recordatorio> ObtenerProximosAVencer(int dias = 7) // Vencen en 7 días o menos
        {
            return _recordatorios
                .Where(r => r.FechaVencimiento > DateTime.Now &&
                            r.FechaVencimiento <= DateTime.Now.AddDays(dias))
                .OrderBy(r => r.FechaVencimiento)
                .ToList();
        }

        public List<Recordatorio> ObtenerCriticos() // Vencen >=7 días y Prioridad Alta
        {
            return _recordatorios
                .Where(r => r.FechaVencimiento <= DateTime.Now.AddDays(7) &&
                            r.Prioridad == Prioridad.Alta)
                .OrderBy(r => r.FechaVencimiento)
                .ToList();
        }
    }
}
