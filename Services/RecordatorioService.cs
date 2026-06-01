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

        public List<Recordatorio> ObtenerVencidos()
        {
            return _recordatorios
                .Where(r => r.FechaVencimiento < DateTime.Now)
                .OrderBy(r => r.FechaVencimiento)
                .ToList();
        }

        public List<Recordatorio> ObtenerProximosAVencer(int dias = 7)
        {
            var ahora = DateTime.Now;
            return _recordatorios
                .Where(r => r.FechaVencimiento > ahora &&
                            r.FechaVencimiento <= ahora.AddDays(dias) &&
                            r.Prioridad != Prioridad.Alta)
                .OrderBy(r => r.FechaVencimiento)
                .ToList();
        }

        public List<Recordatorio> ObtenerCriticos()
        {
            var ahora = DateTime.Now;
            return _recordatorios
                .Where(r => r.FechaVencimiento < ahora ||
                           (r.FechaVencimiento <= ahora.AddDays(7) && r.Prioridad == Prioridad.Alta))
                .OrderBy(r => r.FechaVencimiento)
                .ToList();
        }
    }
}
