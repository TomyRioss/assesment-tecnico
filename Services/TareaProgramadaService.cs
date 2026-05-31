using AssesmentTecnico.Models;

namespace AssesmentTecnico.Services
{
    public class TareaProgramadaService
    {
        private readonly IaService _iaService;
        private readonly EmailService _emailService;
        private readonly FcmService _fcmService;

        public TareaProgramadaService()
        {
            _iaService    = new IaService();
            _emailService = new EmailService();
            _fcmService   = new FcmService();
        }

        public async Task IniciarAsync(List<Recordatorio> recordatorios)
        {
            while (true)
            {
                Console.WriteLine($"\n[SCHEDULER] Ejecutando ciclo: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

                var invalidos = recordatorios
                    .Where(r => string.IsNullOrEmpty(r.TipoVencimiento) || r.ConsorcioId <= 0)
                    .ToList();

                foreach (var r in invalidos)
                    Console.WriteLine($"[WARN] Recordatorio Id {r.Id} ignorado: datos insuficientes.");

                var recordatoriosValidos = recordatorios.Except(invalidos).ToList();

                var resultadoIa = await _iaService.AnalizarRecordatoriosAsync(recordatoriosValidos);

                foreach (var p in resultadoIa.Prioridades)
                {
                    var r = recordatoriosValidos.FirstOrDefault(r => r.Id == p.Id);
                    if (r != null) r.Prioridad = p.Prioridad;
                }

                Console.WriteLine($"[IA] Resumen: {resultadoIa.Resumen}");

                var service         = new RecordatorioService(recordatoriosValidos);
                var criticos        = service.ObtenerCriticos();
                var proximosAVencer = service.ObtenerProximosAVencer();

                await _emailService.EnviarResumenAsync(criticos, proximosAVencer, resultadoIa.Resumen);

                foreach (var r in criticos.Concat(proximosAVencer).DistinctBy(r => r.Id))
                    r.Estado = EstadoRecordatorio.Notificado;

                await _fcmService.EnviarResumenAsync(criticos, proximosAVencer, resultadoIa.Resumen);

                Console.WriteLine($"[SCHEDULER] Ciclo completado. Próxima ejecución en 24 horas.");
                await Task.Delay(TimeSpan.FromHours(24));
            }
        }
    }
}
