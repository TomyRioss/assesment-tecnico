using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssesmentTecnico.Models;

namespace AssesmentTecnico.Services
{
    public class IaService
    {
        private readonly string _apiKey;

        public IaService()
        {
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
        }

        public async Task<ResultadoIa> AnalizarRecordatoriosAsync(List<Recordatorio> recordatorios)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                Console.WriteLine("[IA] SIMULACIÓN: sin API key configurada.");
                return new ResultadoIa
                {
                    Resumen     = "[IA] Resumen simulado: hay recordatorios pendientes que requieren atención.",
                    Prioridades = recordatorios.Select(r => new PrioridadAsignada { Id = r.Id, Prioridad = r.Prioridad }).ToList()
                };
            }

            var listaFormateada = string.Join("\n", recordatorios.Select(r =>
                $"- Id: {r.Id} | Tipo: {r.TipoVencimiento} | Consorcio: {r.ConsorcioId} | Vencimiento: {r.FechaVencimiento:dd/MM/yyyy} | Días restantes: {(r.FechaVencimiento - DateTime.Now).Days}"));

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

            var userPrompt  = $"Analizá estos recordatorios:\n{listaFormateada}";
            var requestBody = new
            {
                model           = "gpt-4o-mini",
                response_format = new { type = "json_object" },
                messages        = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = userPrompt   }
                }
            };

            var jsonSerializado = JsonSerializer.Serialize(requestBody);
            ResultadoIa? resultado = null;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            await RetryHelper.EjecutarConReintentos(async () =>
            {
                var content  = new StringContent(jsonSerializado, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"HTTP {response.StatusCode}");

                var responseJson   = await response.Content.ReadAsStringAsync();
                var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseJson);
                var resultadoJson  = openAiResponse?.Choices?.Count > 0
                    ? openAiResponse.Choices[0]?.Message?.Content ?? string.Empty
                    : string.Empty;
                var resultadoRaw   = JsonSerializer.Deserialize<ResultadoIaJson>(resultadoJson);

                resultado = new ResultadoIa
                {
                    Resumen     = resultadoRaw?.Resumen ?? string.Empty,
                    Prioridades = resultadoRaw?.Prioridades?.Select(p => new PrioridadAsignada
                    {
                        Id        = p.Id,
                        Prioridad = Enum.TryParse<Prioridad>(p.Prioridad, out var prioridad) ? prioridad : Prioridad.Media
                    }).ToList() ?? new()
                };
            }, "[IA]");

            return resultado ?? Fallback(recordatorios);
        }

        private ResultadoIa Fallback(List<Recordatorio> recordatorios) => new ResultadoIa
        {
            Resumen     = "[IA] No disponible. Revisá los recordatorios manualmente.",
            Prioridades = recordatorios.Select(r => new PrioridadAsignada { Id = r.Id, Prioridad = r.Prioridad }).ToList()
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

        private class ResultadoIaJson
        {
            [JsonPropertyName("prioridades")]
            public List<PrioridadItemJson>? Prioridades { get; set; }

            [JsonPropertyName("resumen")]
            public string? Resumen { get; set; }
        }

        private class PrioridadItemJson
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("prioridad")]
            public string Prioridad { get; set; } = string.Empty;
        }
    }
}
