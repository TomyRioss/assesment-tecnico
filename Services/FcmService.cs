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

        public async Task<bool> SendSummaryAsync(List<Reminder> critical, List<Reminder> upcoming, string aiSummary = "")
        {
            var title          = "[AdminProp] Resumen de vencimientos";
            var criticalPart   = critical.Any()
                ? string.Join(" | ", critical.Select(r => $"{r.ExpiryType} (Consorcio {r.CondoId})"))
                : "Sin críticos.";
            var upcomingPart   = upcoming.Any()
                ? string.Join(" | ", upcoming.Select(r => $"{r.ExpiryType} vence en {(r.ExpiryDate - DateTime.Now).Days} días"))
                : "Sin próximos a vencer.";
            var message        = string.IsNullOrEmpty(aiSummary)
                ? $"CRÍTICOS: {criticalPart} — PRÓXIMOS: {upcomingPart}"
                : aiSummary;

            if (string.IsNullOrEmpty(_projectId) || string.IsNullOrEmpty(_accessToken) || string.IsNullOrEmpty(_deviceToken))
            {
                Console.WriteLine("[FCM] SIMULACIÓN: notificación push preparada.");
                Console.WriteLine($"[FCM] Título:  {title}");
                Console.WriteLine($"[FCM] Mensaje: {message}");
                Console.WriteLine("[FCM] En producción, este mensaje se entregaría al dispositivo con token: [FCM_DEVICE_TOKEN]");
                return true;
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return await RetryHelper.ExecuteWithRetry(async () =>
            {
                var payload = new
                {
                    message = new
                    {
                        token        = _deviceToken,
                        notification = new { title, body = message }
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
