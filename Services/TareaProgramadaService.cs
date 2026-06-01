using AssesmentTecnico.Models;

namespace AssesmentTecnico.Services
{
    public class TareaProgramadaService
    {
        private readonly IaService           _iaService;
        private readonly EmailService        _emailService;
        private readonly FcmService          _fcmService;
        private readonly RecordatorioService _recordatorioService;

        public TareaProgramadaService(List<Recordatorio> recordatorios)
        {
            _iaService           = new IaService();
            _emailService        = new EmailService();
            _fcmService          = new FcmService();
            _recordatorioService = new RecordatorioService(recordatorios);
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

                var criticos        = _recordatorioService.ObtenerCriticos();
                var proximosAVencer = _recordatorioService.ObtenerProximosAVencer();

                var emailOk = await _emailService.EnviarResumenAsync(criticos, proximosAVencer, resultadoIa.Resumen);
                var fcmOk   = await _fcmService.EnviarResumenAsync(criticos, proximosAVencer, resultadoIa.Resumen);

                if (emailOk && fcmOk)
                {
                    foreach (var r in criticos.Concat(proximosAVencer).DistinctBy(r => r.Id))
                        r.Estado = EstadoRecordatorio.Notificado;
                }
                else
                {
                    Console.WriteLine("[SCHEDULER] Estado no actualizado: uno o más servicios fallaron.");
                }

                Console.WriteLine($"[SCHEDULER] Ciclo completado. Próxima ejecución en 24 horas.");
                await Task.Delay(TimeSpan.FromHours(24));
            }
        }
    }
}
