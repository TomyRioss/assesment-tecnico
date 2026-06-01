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
        Prioridad = Prioridad.Media
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
        Prioridad = Prioridad.Media
    }
};

var scheduler = new TareaProgramadaService();
await scheduler.IniciarAsync(recordatorios);
