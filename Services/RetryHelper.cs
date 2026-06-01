namespace AssesmentTecnico.Services
{
    public static class RetryHelper
    {
        public static async Task<bool> EjecutarConReintentos(Func<Task> accion, string prefijo)
        {
            int intentos = 3;
            int delayMs  = 2000;

            for (int i = 0; i < intentos; i++)
            {
                try
                {
                    await accion();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{prefijo} [REINTENTO {i + 1}/{intentos}] Error: {ex.Message}");
                    if (i < intentos - 1)
                        await Task.Delay(delayMs * (int)Math.Pow(2, i));
                }
            }

            Console.WriteLine($"{prefijo} Todos los reintentos fallaron.");
            return false;
        }
    }
}
