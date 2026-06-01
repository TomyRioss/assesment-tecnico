using DotNetEnv;
using AssesmentTecnico.Models;
using AssesmentTecnico.Services;

Env.Load();

var reminders = new List<Reminder>
{
    new Reminder
    {
        Id          = 1,
        ExpiryType  = "Seguro contra incendio",
        ExpiryDate  = DateTime.Now.AddDays(-3),
        Description = "Póliza nro. 4521",
        CondoId     = 1,
        Status      = ReminderStatus.Pending,
        Priority    = Priority.Medium
    },
    new Reminder
    {
        Id          = 2,
        ExpiryType  = "Mantenimiento de ascensor",
        ExpiryDate  = DateTime.Now.AddDays(5),
        Description = "Empresa TecnoAscensores",
        CondoId     = 2,
        Status      = ReminderStatus.Pending,
        Priority    = Priority.Medium
    },
    new Reminder
    {
        Id          = 3,
        ExpiryType  = "Habilitación municipal",
        ExpiryDate  = DateTime.Now.AddDays(30),
        Description = "Renovación anual",
        CondoId     = 1,
        Status      = ReminderStatus.Pending,
        Priority    = Priority.Medium
    }
};

var service = new ReminderService(reminders);

Console.WriteLine("=== VENCIDOS ===");
foreach (var r in service.GetOverdue())
    Console.WriteLine($"- {r.ExpiryType} (Consorcio {r.CondoId})");

Console.WriteLine("\n=== PRÓXIMOS A VENCER ===");
foreach (var r in service.GetUpcoming())
    Console.WriteLine($"- {r.ExpiryType} vence el {r.ExpiryDate:dd/MM/yyyy}");

Console.WriteLine("\n=== CRÍTICOS ===");
foreach (var r in service.GetCritical())
    Console.WriteLine($"- {r.ExpiryType} | Prioridad: {r.Priority}");

Console.WriteLine();

var scheduler = new SchedulerService();
await scheduler.StartAsync(reminders);
