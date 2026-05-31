using DotNetEnv;
using AssesmentTecnico.Models;
using AssesmentTecnico.Services;

Env.Load();

var recordatorios = new List<Recordatorio>
{
    new Recordatorio
    {
        Id = 1,
        TipoVencimiento = "Seguro contra incendio",
        FechaVencimiento = DateTime.Now.AddDays(-3),
        Descripcion = "Póliza nro. 4521",
        ConsorcioId = 1,
        Estado = EstadoRecordatorio.Pendiente,
        Prioridad = Prioridad.Alta
    },
    new Recordatorio
    {
        Id = 2,
        TipoVencimiento = "Mantenimiento de ascensor",
        FechaVencimiento = DateTime.Now.AddDays(5),
        Descripcion = "Empresa TecnoAscensores",
        ConsorcioId = 2,
        Estado = EstadoRecordatorio.Pendiente,
        Prioridad = Prioridad.Media
    },
    new Recordatorio
    {
        Id = 3,
        TipoVencimiento = "Habilitación municipal",
        FechaVencimiento = DateTime.Now.AddDays(30),
        Descripcion = "Renovación anual",
        ConsorcioId = 1,
        Estado = EstadoRecordatorio.Pendiente,
        Prioridad = Prioridad.Baja
    }
};

var service = new RecordatorioService(recordatorios);

Console.WriteLine("=== VENCIDOS ===");
foreach (var r in service.ObtenerVencidos())
    Console.WriteLine($"- {r.TipoVencimiento} (Consorcio {r.ConsorcioId})");

Console.WriteLine("\n=== PRÓXIMOS A VENCER ===");
foreach (var r in service.ObtenerProximosAVencer())
    Console.WriteLine($"- {r.TipoVencimiento} vence el {r.FechaVencimiento:dd/MM/yyyy}");

Console.WriteLine("\n=== CRÍTICOS ===");
foreach (var r in service.ObtenerCriticos())
    Console.WriteLine($"- {r.TipoVencimiento} | Prioridad: {r.Prioridad}");

var criticos        = service.ObtenerCriticos();
var proximosAVencer = service.ObtenerProximosAVencer();

Console.WriteLine("\n=== ENVIANDO RESUMEN POR EMAIL ===");
var emailService = new EmailService();
await emailService.EnviarResumenAsync(criticos, proximosAVencer);

Console.WriteLine("\n=== ENVIANDO RESUMEN PUSH (FCM) ===");
var fcmService = new FcmService();
await fcmService.EnviarResumenAsync(criticos, proximosAVencer);
