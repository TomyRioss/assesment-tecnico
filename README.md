# AssesmentTecnico

## Cómo ejecutar

1. Ejecutar `git clone https://github.com/TomyRioss/assesment-tecnico.git`
2. Navegar a la carpeta del proyecto `cd assesmenttecnico`
3. Correr `dotnet restore`
4. Correr `dotnet run`
5. Copiar `.env.example` a `.env` y completar las credenciales (opcional)

El sistema funciona sin credenciales: sin API key de OpenAI corre en modo simulación con prioridades estáticas, sin credenciales SMTP imprime el email en consola, y sin credenciales FCM imprime la notificación push en consola.
