using System.Reflection;
using Records.Persons.Application;
using Records.Persons.Infra.Configuration.DependencyInjection;
using Records.Shared.Infra.Http;
using Records.Shared.Infra.Http.DependencyInjection;
using Records.Shared.Infra.OpenApi.DependencyInjection;
using Records.Shared.Infra.Serilog.DependencyInjection;
using Records.Shared.Mediator.DependencyInjection;
using Scalar.AspNetCore;

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

        IServiceCollection services = builder.Services;

        // Configures and registers Serilog as the logger.
        builder.AddSerilogCustom();

        // Registers the necessary configurations with the DI framework.
        services.AddConfiguration(builder.Configuration);

        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Registers the necessary services for the mediator (commands, queries and domain events) with the DI framework.
        services.AddMediator(typeof(AssemblyReference).Assembly);

        services.AddEndpoints(Assembly.GetExecutingAssembly());

        // Adds services to the container.
        services.AddAuthorization();

        // Configura y registra la generación del documento OpenAPI.
        //
        // NOTE: AddOpenApi se llama acá (con "v1" como literal) y no en un helper de la librería
        // compartida porque el source generator que lee los comentarios XML de los DTOs (<summary>,
        // <example>, etc.) resuelve esos comentarios según las ProjectReference del proyecto donde
        // está el call site. Ver el comentario en ConfigureOpenApiCustom para más detalle.
        const string description = "Proyecto modelo base aplicando estándares de estructura, " +
                                    "codificación, reglas y documentación de código.";
        services.AddOpenApi("v1", options => options.ConfigureOpenApiCustom(
            title: "Demo proyecto Persons",
            version: "v1",
            description: description));

        // Enables API explorer for endpoints.
        services.AddEndpointsApiExplorer();

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
