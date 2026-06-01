namespace AssesmentTecnico.Services
{
    public static class RetryHelper
    {
        public static async Task<bool> ExecuteWithRetry(Func<Task> action, string prefix)
        {
            int attempts = 3;
            int delayMs  = 2000;

            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    await action();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{prefix} [REINTENTO {i + 1}/{attempts}] Error: {ex.Message}");
                    if (i < attempts - 1)
                        await Task.Delay(delayMs * (1 << i));
                }
            }

            Console.WriteLine($"{prefix} Todos los reintentos fallaron.");
            return false;
        }
    }
}
