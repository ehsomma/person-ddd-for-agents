#region Usings

using System.Reflection;
using Records.Persons.Infra.Configuration.DependencyInjection;
using Records.Shared.Infra.Http;
using Records.Shared.Infra.Http.DependencyInjection;
using Records.Shared.Infra.OpenApi.DependencyInjection;
using Records.Shared.Infra.Serilog.DependencyInjection;
using Scalar.AspNetCore;

#endregion

namespace Records.Persons.Api.V1;

/// <summary>
/// Entry point of the application.
/// </summary>
internal sealed class Program
{
    /// <summary>
    /// Creates an instance of the web application’s host. The host is responsible for
    /// bootstrapping the application and setting up the necessary services and middleware.
    ///
    /// It calls the Run method on the host, which starts the web server and listens for
    /// incoming HTTP requests.
    /// </summary>
    /// <param name="args">Arguments parameter that can be used to retrieve the arguments passed while running the application.</param>
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Configures and registers Serilog as the logger.
        builder.AddSerilogCustom();

        // Registers the necessary configurations with the DI framework.
        builder.Services.AddConfiguration(builder.Configuration);

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

        // Adds services to the container.
        builder.Services.AddAuthorization();

        // Configura y registra la generación del documento OpenAPI.
        const string description = "Proyecto modelo base aplicando estándares de estructura, " +
                                    "codificación, reglas y documentación de código.";
        builder.Services.AddOpenApiCustom(
            title: "Demo proyecto Persons",
            version: "v1",
            description: description);

        // Enables API explorer for endpoints.
        builder.Services.AddEndpointsApiExplorer();

        WebApplication app = builder.Build();

        // Builder vacío: nuestro handler escribe la respuesta. Sin esto y al no usar el estándar
        // `ProblemDetails` para devolver errores, falla al arrancar.
        app.UseExceptionHandler(_ => { });

        // Configure the HTTP request pipeline.
        app.MapOpenApi();

        // UI en /scalar/v1.
        app.MapScalarApiReference();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        ////app.UseSerilogRequestLogging();

        // Mapea los endpoints.
        app.MapEndpoints();

        app.Run();
    }
}
