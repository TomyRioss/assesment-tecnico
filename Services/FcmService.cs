using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DotNetEnv;
using AssesmentTecnico.Models;

namespace AssesmentTecnico.Services
{
    public class FcmService
    {
        private readonly string _projectId;
        private readonly string _accessToken;
        private readonly string _deviceToken;

        public FcmService()
        {
            Env.Load();

            _projectId   = Environment.GetEnvironmentVariable("FCM_PROJECT_ID")   ?? string.Empty;
            _accessToken = Environment.GetEnvironmentVariable("FCM_ACCESS_TOKEN") ?? string.Empty;
            _deviceToken = Environment.GetEnvironmentVariable("FCM_DEVICE_TOKEN") ?? string.Empty;
        }

        public async Task EnviarResumenAsync(List<Recordatorio> criticos, List<Recordatorio> proximosAVencer, string resumenIa = "")
        {
            var seccionCriticos = criticos.Any()
                ? string.Join(" | ", criticos.Select(r => $"⚠️ {r.TipoVencimiento} (Consorcio {r.ConsorcioId})"))
                : "Sin críticos.";

            var seccionProximos = proximosAVencer.Any()
                ? string.Join(" | ", proximosAVencer.Select(r => $"🔔 {r.TipoVencimiento} vence en {(r.FechaVencimiento - DateTime.Now).Days} días"))
                : "Sin próximos a vencer.";

            var titulo  = "[AdminProp] Resumen de vencimientos";
            var mensaje = string.IsNullOrEmpty(resumenIa)
                ? $"CRÍTICOS: {seccionCriticos} — PRÓXIMOS: {seccionProximos}"
                : resumenIa;

            if (string.IsNullOrEmpty(_projectId) || string.IsNullOrEmpty(_accessToken))
            {
                Console.WriteLine("[FCM] SIMULACIÓN: notificación push preparada.");
                Console.WriteLine($"[FCM] Título:  {titulo}");
                Console.WriteLine($"[FCM] Mensaje: {mensaje}");
                Console.WriteLine("[FCM] En producción, este mensaje se entregaría al dispositivo con token: [FCM_DEVICE_TOKEN]");
                return;
            }

            await EjecutarConReintentos(async () =>
            {
                var payload = new
                {
                    message = new
                    {
                        token        = _deviceToken,
                        notification = new { title = titulo, body = mensaje }
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content     = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                var url      = $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send";
                var response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"HTTP {response.StatusCode}");

                Console.WriteLine("[FCM] Resumen de vencimientos enviado correctamente.");
            }, "[FCM]");
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
