using AssesmentTecnico.Models;

namespace AssesmentTecnico.Services
{
    public class SchedulerService
    {
        private readonly AiService    _aiService;
        private readonly EmailService _emailService;
        private readonly FcmService   _fcmService;

        public SchedulerService()
        {
            _aiService    = new AiService();
            _emailService = new EmailService();
            _fcmService   = new FcmService();
        }

        public async Task StartAsync(List<Reminder> reminders)
        {
            while (true)
            {
                Console.WriteLine($"\n[SCHEDULER] Ejecutando ciclo: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

                var invalid = reminders
                    .Where(r => string.IsNullOrEmpty(r.ExpiryType) || r.CondoId <= 0)
                    .ToList();

                foreach (var r in invalid)
                    Console.WriteLine($"[WARN] Recordatorio Id {r.Id} ignorado: datos insuficientes.");

                var validReminders = reminders.Except(invalid).ToList();

                var aiResult = await _aiService.AnalyzeRemindersAsync(validReminders);

                foreach (var p in aiResult.Priorities)
                {
                    var r = validReminders.FirstOrDefault(r => r.Id == p.Id);
                    if (r != null) r.Priority = p.Priority;
                }

                Console.WriteLine($"[IA] Resumen: {aiResult.Summary}");

                var reminderService = new ReminderService(validReminders);
                var critical        = reminderService.GetCritical();
                var upcoming        = reminderService.GetUpcoming();

                var emailOk = await _emailService.SendSummaryAsync(critical, upcoming, aiResult.Summary);
                var fcmOk   = await _fcmService.SendSummaryAsync(critical, upcoming, aiResult.Summary);

                if (emailOk && fcmOk)
                {
                    foreach (var r in critical.Concat(upcoming).DistinctBy(r => r.Id))
                        r.Status = ReminderStatus.Notified;
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
