using System.Net;
using System.Net.Mail;
using AssesmentTecnico.Models;

namespace AssesmentTecnico.Services
{
    public class EmailService
    {
        private readonly string _sender;
        private readonly string _password;
        private readonly string _recipient;

        public EmailService()
        {
            _sender    = Environment.GetEnvironmentVariable("SMTP_EMAIL")        ?? string.Empty;
            _password  = Environment.GetEnvironmentVariable("SMTP_PASSWORD")     ?? string.Empty;
            _recipient = Environment.GetEnvironmentVariable("SMTP_DESTINATARIO") ?? string.Empty;
        }

        public async Task<bool> SendSummaryAsync(List<Reminder> critical, List<Reminder> upcoming, string aiSummary = "")
        {
            var criticalSection = critical.Any()
                ? string.Join("\n", critical.Select(r => $"  {r.ExpiryType} (Consorcio {r.CondoId})"))
                : "  Sin recordatorios críticos.";

            var upcomingSection = upcoming.Any()
                ? string.Join("\n", upcoming.Select(r => $"  {r.ExpiryType} vence en {(r.ExpiryDate - DateTime.Now).Days} días"))
                : "  Sin recordatorios próximos a vencer.";

            var subject = "[AdminProp] Resumen de vencimientos";
            var body    = $"""
                Estimado administrador,

                === ANÁLISIS IA ===
                {aiSummary}

                === CRÍTICOS / VENCIDOS ===
                {criticalSection}

                === PRÓXIMOS A VENCER ===
                {upcomingSection}

                Por favor tome las acciones necesarias a la brevedad.

                AdminProp - Sistema de recordatorios
                """;

            if (string.IsNullOrEmpty(_sender) || string.IsNullOrEmpty(_recipient)) // FALLBACK = NO CREDENCIALES
            {
                Console.WriteLine("[EMAIL] SIMULACIÓN: email preparado.");
                Console.WriteLine($"[EMAIL] Asunto: {subject}");
                Console.WriteLine($"[EMAIL] Cuerpo:\n{body}");
                Console.WriteLine("[EMAIL] En producción, este email se enviaría con credenciales SMTP configuradas.");
                return true;
            }

            return await RetryHelper.ExecuteWithRetry(async () =>
            {
                using var message = new MailMessage(_sender, _recipient, subject, body);
                using var client  = new SmtpClient("smtp.gmail.com", 587)
                {
                    UseDefaultCredentials = false,
                    Credentials           = new NetworkCredential(_sender, _password),
                    EnableSsl             = true
                };

                await client.SendMailAsync(message);
                Console.WriteLine("[EMAIL] Resumen de vencimientos enviado correctamente.");
            }, "[EMAIL]");
        }
    }
}
