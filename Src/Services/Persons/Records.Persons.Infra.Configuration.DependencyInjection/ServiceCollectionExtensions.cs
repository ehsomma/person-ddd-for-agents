#region Usings

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Records.Persons.Shared.Entities;

#endregion

namespace Records.Persons.Shared.Configuration.DependencyInjection;

/// <summary>
/// Extensions methods for dependency injection.
/// </summary>
public static class DependencyInjection
{
    #region Public methods

    /// <summary>
    /// Registers the necessary configurations with the DI framework.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        /*
        Nuget packages required to use IServiceCollection (DI):
        • Microsoft.Extensions.DependencyInjection.Abstractions // DI

        Nuget packages required to use IConfiguration, .GetSection() and .Get():
        • Microsoft.Extensions.Configuration.Abstractions // IConfiguration
        • Microsoft.Extensions.Options // .ValidateOnStart()
        • Microsoft.Extensions.Options.ConfigurationExtensions // .Bind()
        • Microsoft.Extensions.Options.DataAnnotations // .ValidateDataAnnotations()
        */

        // To inject IOptions<AppSettings> via constructor on methods or clases.
        // To inject IOptionsSnapshot<AppSettings> via constructor on methods or clases.
        // To inject IOptionsMonitor<AppSettings> via constructor on methods or clases.
        services
            .AddOptions<PersonsAppSettings>()
            .Bind(configuration.GetSection(PersonsAppSettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.Setting1),
                "Falta la propiedad de configuración requerida 'AppSettings.Persons.Setting1'.")
            .ValidateOnStart();

        // AppSettingsService (singleton).
        ////services.AddSingleton<IAppSettingsService, AppSettingsService>();

        // Ejemplos si se quiere usar desde aquí:
        ////PersonsAppSettings? settings = configuration.GetSection(PersonsAppSettings.SectionName).Get<PersonsAppSettings>();
        ////string? setting1 = settings?.Setting1;
        ////string? connString = configuration.GetConnectionString("Default"); // lee ConnectionStrings:Default
        ////string? setting2 = configuration["Persons:Setting2"];

        return services;
    }

    #endregion
}
