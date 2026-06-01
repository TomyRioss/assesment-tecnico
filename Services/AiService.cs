using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssesmentTecnico.Models;

namespace AssesmentTecnico.Services
{
    public class AiService
    {
        private readonly string _apiKey;

        public AiService()
        {
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
        }

        public async Task<AiResult> AnalyzeRemindersAsync(List<Reminder> reminders)
        {
            if (string.IsNullOrEmpty(_apiKey)) // FALLBACK = NO CREDENCIALES
            {
                Console.WriteLine("[IA] SIMULACIÓN: sin API key configurada.");
                return new AiResult
                {
                    Summary    = "[IA] Resumen simulado: hay recordatorios pendientes que requieren atención.",
                    Priorities = reminders.Select(r => new AssignedPriority { Id = r.Id, Priority = r.Priority }).ToList()
                };
            }

            var formattedList = string.Join("\n", reminders.Select(r =>
                $"- Id: {r.Id} | Tipo: {r.ExpiryType} | Consorcio: {r.CondoId} | Vencimiento: {r.ExpiryDate:dd/MM/yyyy} | Días restantes: {(r.ExpiryDate - DateTime.Now).Days}"));

            var systemPrompt = """
                Sos un asistente de gestión de consorcios inmobiliarios.
                Analizás recordatorios de vencimientos y devolvés ÚNICAMENTE un JSON válido con esta estructura exacta:
                {
                  "prioridades": [
                    { "id": 1, "prioridad": "Alta" },
                    { "id": 2, "prioridad": "Media" }
                  ],
                  "resumen": "Texto en lenguaje natural explicando el estado general de los vencimientos y recomendaciones."
                }
                Los valores válidos para prioridad son: "Alta", "Media", "Baja".
                Asigná Alta si vence en 7 días o menos o ya venció. Media si vence entre 8 y 30 días. Baja si vence en más de 30 días.
                """;

            var userPrompt  = $"Analizá estos recordatorios:\n{formattedList}";
            var requestBody = new
            {
                model           = "gpt-4o-mini",
                response_format = new { type = "json_object" },
                messages        = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = userPrompt   }
                }
            }; // 4o-Mini = Barato y Excelente para clasificación. Json_object = Modelo solo responde en formato json.

            var serializedJson = JsonSerializer.Serialize(requestBody);
            AiResult? result   = null;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            await RetryHelper.ExecuteWithRetry(async () =>
            {
                var content  = new StringContent(serializedJson, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"HTTP {response.StatusCode}");

                var responseJson   = await response.Content.ReadAsStringAsync();
                var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseJson);
                var resultJson     = openAiResponse?.Choices?.Count > 0
                    ? openAiResponse.Choices[0]?.Message?.Content ?? string.Empty
                    : string.Empty;
                var rawResult      = JsonSerializer.Deserialize<AiResultJson>(resultJson);

                result = new AiResult
                {
                    Summary    = rawResult?.Summary ?? string.Empty,
                    Priorities = rawResult?.Priorities?.Select(p => new AssignedPriority
                    {
                        Id       = p.Id,
                        Priority = Enum.TryParse<Priority>(p.Priority, out var priority) ? priority : Priority.Medium
                    }).ToList() ?? new()
                };
            }, "[IA]");

            return result ?? Fallback(reminders);
        }

        private AiResult Fallback(List<Reminder> reminders) => new AiResult
        {
            Summary    = "[IA] No disponible. Revisá los recordatorios manualmente.",
            Priorities = reminders.Select(r => new AssignedPriority { Id = r.Id, Priority = r.Priority }).ToList()
        };

        private class OpenAiResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice>? Choices { get; set; }

            public class Choice
            {
                [JsonPropertyName("message")]
                public Message? Message { get; set; }
            }

            public class Message
            {
                [JsonPropertyName("content")]
                public string? Content { get; set; }
            }
        }

        private class AiResultJson
        {
            [JsonPropertyName("prioridades")]
            public List<PriorityItemJson>? Priorities { get; set; }

            [JsonPropertyName("resumen")]
            public string? Summary { get; set; }
        }

        private class PriorityItemJson
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("prioridad")]
            public string Priority { get; set; } = string.Empty;
        }
    }
}
