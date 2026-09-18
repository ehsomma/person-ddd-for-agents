#region Usings

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;

#endregion

namespace Records.Shared.Infra.Serilog.DependencyInjection;

/// <summary>
/// Extensions methods for dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Public methods

    /// <summary>
    /// Configures and registers Serilog as the logger, combining appsettings.json
    /// configuration with code-based enrichers.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <returns>The same web application builder.</returns>
    public static WebApplicationBuilder AddSerilogCustom(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Permite cambiar el nivel de log en runtime. Se inyecta en un endpoint
        // para subir/bajar el verbosity sin reiniciar la app. Se puede inyectar en un endpoint.
        LoggingLevelSwitch loggingLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Information);
        builder.Services.AddSingleton(loggingLevelSwitch);

        builder.Services.AddSerilog((services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(builder.Configuration) // 1. Lee sinks/niveles/overrides desde la sección "Serilog" del appsettings.json.
                .ReadFrom.Services(services) // 2. Permite que el servicio DI resuelva sinks/enrichers si hiciera falta.
                .MinimumLevel.ControlledBy(loggingLevelSwitch) // 3. El switch controla el nivel mínimo (pisa lo que venga del JSON).
                .Enrich.WithThreadId() // 4. Enrichers por código — esto es lo que "no funcionaba" con el appsettings.
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails(
                    new DestructuringOptionsBuilder().WithDefaultDestructurers());
        });

        return builder;
    }

    #endregion
}
