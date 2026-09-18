#region Usings

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Records.Persons.Shared.Configuration;

#endregion

namespace Records.Persons.Infra.Configuration.DependencyInjection;

/// <summary>
/// Extensions methods for dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Definitions

    private const string KeyNotFound = "The '{0}' configuration key was not found.";

    #endregion

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

        To inject IOptions<AppSettings> via constructor on methods or clases.
        To inject IOptionsSnapshot<AppSettings> via constructor on methods or clases.
        To inject IOptionsMonitor<AppSettings> via constructor on methods or clases.
        */

        // NOTE: Con el .Validate() no hace falta tener una clase (tipo ConfigurationManager)
        // que controle si existen las keys de configuración, esto lo reemplaza.
        services
            .AddOptions<PersonsSettings>()
            .Bind(configuration.GetSection(PersonsSettings.SettingsKey))
            .ValidateDataAnnotations()
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.Setting1),
                string.Format(null, KeyNotFound, "AppSettings.Persons.Setting1"))
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
