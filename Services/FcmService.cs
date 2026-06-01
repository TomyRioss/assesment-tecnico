using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
            _projectId   = Environment.GetEnvironmentVariable("FCM_PROJECT_ID")   ?? string.Empty;
            _accessToken = Environment.GetEnvironmentVariable("FCM_ACCESS_TOKEN") ?? string.Empty;
            _deviceToken = Environment.GetEnvironmentVariable("FCM_DEVICE_TOKEN") ?? string.Empty;
        }

        public async Task<bool> EnviarResumenAsync(List<Recordatorio> criticos, List<Recordatorio> proximosAVencer, string resumenIa = "")
        {
            var titulo  = "[AdminProp] Resumen de vencimientos";
            var mensaje = string.IsNullOrEmpty(resumenIa)
                ? $"CRÍTICOS: {(criticos.Any() ? string.Join(" | ", criticos.Select(r => $"{r.TipoVencimiento} (Consorcio {r.ConsorcioId})")) : "Sin críticos.")} — PRÓXIMOS: {(proximosAVencer.Any() ? string.Join(" | ", proximosAVencer.Select(r => $"{r.TipoVencimiento} vence en {(r.FechaVencimiento - DateTime.Now).Days} días")) : "Sin próximos a vencer.")}"
                : resumenIa;

            if (string.IsNullOrEmpty(_projectId) || string.IsNullOrEmpty(_accessToken))
            {
                Console.WriteLine("[FCM] SIMULACIÓN: notificación push preparada.");
                Console.WriteLine($"[FCM] Título:  {titulo}");
                Console.WriteLine($"[FCM] Mensaje: {mensaje}");
                Console.WriteLine("[FCM] En producción, este mensaje se entregaría al dispositivo con token: [FCM_DEVICE_TOKEN]");
                return true;
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return await RetryHelper.EjecutarConReintentos(async () =>
            {
                var payload = new
                {
                    message = new
                    {
                        token        = _deviceToken,
                        notification = new { title = titulo, body = mensaje }
                    }
                };

                var content  = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var url      = $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send";
                var response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"HTTP {response.StatusCode}");

                Console.WriteLine("[FCM] Resumen de vencimientos enviado correctamente.");
            }, "[FCM]");
        }
    }
}
