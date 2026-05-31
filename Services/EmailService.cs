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

        public async Task EnviarResumenAsync(List<Recordatorio> criticos, List<Recordatorio> proximosAVencer, string resumenIa = "")
        {
            var seccionCriticos = criticos.Any()
                ? string.Join("\n", criticos.Select(r => $"  ⚠️ {r.TipoVencimiento} (Consorcio {r.ConsorcioId})"))
                : "  Sin recordatorios críticos.";

            var seccionProximos = proximosAVencer.Any()
                ? string.Join("\n", proximosAVencer.Select(r => $"  🔔 {r.TipoVencimiento} vence en {(r.FechaVencimiento - DateTime.Now).Days} días"))
                : "  Sin recordatorios próximos a vencer.";

            var asunto = "[AdminProp] Resumen de vencimientos";
            var cuerpo = $"""
                Estimado administrador,

                === ANÁLISIS IA ===
                {resumenIa}

                === CRÍTICOS / VENCIDOS ===
                {seccionCriticos}

                === PRÓXIMOS A VENCER ===
                {seccionProximos}

                Por favor tome las acciones necesarias a la brevedad.

                AdminProp - Sistema de recordatorios
                """;

            if (string.IsNullOrEmpty(_remitente) || string.IsNullOrEmpty(_destinatario))
            {
                Console.WriteLine("[EMAIL] SIMULACIÓN: email preparado.");
                Console.WriteLine($"[EMAIL] Asunto: {asunto}");
                Console.WriteLine($"[EMAIL] Cuerpo:\n{cuerpo}");
                Console.WriteLine("[EMAIL] En producción, este email se enviaría con credenciales SMTP configuradas.");
                return;
            }

            await EjecutarConReintentos(async () =>
            {
                using var mensaje = new MailMessage(_remitente, _destinatario, asunto, cuerpo);
                using var cliente = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(_remitente, _password),
                    EnableSsl   = true
                };

                await cliente.SendMailAsync(mensaje);
                Console.WriteLine("[EMAIL] Resumen de vencimientos enviado correctamente.");
            }, "[EMAIL]");
        }

        private async Task EjecutarConReintentos(Func<Task> accion, string prefijo)
        {
            int intentos = 3;
            int delayMs  = 2000;

            for (int i = 0; i < intentos; i++)
            {
                try
                {
                    await accion();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{prefijo} [REINTENTO {i + 1}/{intentos}] Error: {ex.Message}");
                    if (i < intentos - 1)
                        await Task.Delay(delayMs * (int)Math.Pow(2, i));
                }
            }
        }
    }
}
