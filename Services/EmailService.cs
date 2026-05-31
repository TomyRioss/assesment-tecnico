using System.Net;
using System.Net.Mail;
using DotNetEnv;
using AssesmentTecnico.Models;

namespace AssesmentTecnico.Services
{
    public class EmailService
    {
        private readonly string _remitente;
        private readonly string _password;
        private readonly string _destinatario;

        public EmailService()
        {
            Env.Load();

            _remitente    = Environment.GetEnvironmentVariable("SMTP_EMAIL")        ?? string.Empty;
            _password     = Environment.GetEnvironmentVariable("SMTP_PASSWORD")     ?? string.Empty;
            _destinatario = Environment.GetEnvironmentVariable("SMTP_DESTINATARIO") ?? string.Empty;
        }

        public async Task EnviarResumenAsync(List<Recordatorio> criticos, List<Recordatorio> proximosAVencer)
        {
            var seccionCriticos = criticos.Any()
                ? string.Join("\n", criticos.Select(r =>
                    $"  ⚠️ {r.TipoVencimiento} (Consorcio {r.ConsorcioId})"))
                : "  Sin recordatorios críticos.";

            var seccionProximos = proximosAVencer.Any()
                ? string.Join("\n", proximosAVencer.Select(r =>
                    $"  🔔 {r.TipoVencimiento} vence en {(r.FechaVencimiento - DateTime.Now).Days} días"))
                : "  Sin recordatorios próximos a vencer.";

            var asunto = "[AdminProp] Resumen de vencimientos";
            var cuerpo = $"""
                Estimado administrador,

                === CRÍTICOS / VENCIDOS ===
                {seccionCriticos}

                === PRÓXIMOS A VENCER ===
                {seccionProximos}

                Por favor tome las acciones necesarias a la brevedad.

                AdminProp - Sistema de recordatorios
                """;

            if (string.IsNullOrEmpty(_remitente) || string.IsNullOrEmpty(_destinatario))
            {
                Console.WriteLine("[EMAIL] CREDENCIALES NO ENCONTRADAS, SIMULACIÓN CARGADA: email preparado.");
                Console.WriteLine($"[EMAIL] Asunto: {asunto}");
                Console.WriteLine($"[EMAIL] Cuerpo:\n{cuerpo}");
                Console.WriteLine("[EMAIL] En producción, este email se enviaría con credenciales SMTP configuradas.");
                return;
            }

            using var mensaje = new MailMessage(_remitente, _destinatario, asunto, cuerpo);

            using var cliente = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(_remitente, _password),
                EnableSsl   = true
            };

            try
            {
                await cliente.SendMailAsync(mensaje);
                Console.WriteLine("[EMAIL] Resumen de vencimientos enviado correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL] Error al enviar resumen: {ex.Message}");
            }
        }
    }
}
