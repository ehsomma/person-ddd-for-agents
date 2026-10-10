using System.Reflection;
using BuildingBlocks.Application.DependencyInjection;
using BuildingBlocks.Infra.Http;
using BuildingBlocks.Infra.Http.DependencyInjection;
using BuildingBlocks.Infra.Mappings.DependencyInjection;
using BuildingBlocks.Infra.OpenApi.DependencyInjection;
using BuildingBlocks.Infra.Persistence.DependencyInjection;
using BuildingBlocks.Infra.Serilog.DependencyInjection;
using BuildingBlocks.Mediator.DependencyInjection;
using Records.Persons.Application;
using Records.Persons.Domain.PersonAggregate.Services;
using Records.Persons.Infra.Configuration.DependencyInjection;
using Records.Persons.Infra.Persistence.Sql.PersonAggregate.Mappers;
using Records.Persons.Infra.Persistence.Sql.PersonAggregate.Repository;
using Records.Persons.Infra.Queries.Sql.Person.GetPersonById;
using Scalar.AspNetCore;

namespace Records.Persons.Api.V1;

/// <summary>
/// Entry point of the application.
/// </summary>
internal sealed class Program
{
    #region Public methods

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

        // Hace que minimal APIs lance BadHttpRequestException ante un request mal formado (body faltante, JSON
        // inválido, header requerido ausente, etc.) en todos los ambientes, para que el GlobalExceptionHandler
        // devuelva el ErrorResponse. Por defecto solo lo hace en Development; en el resto devuelve un 400 vacío.
        services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

        // Registers the necessary services for the mediator (commands and queries) with the DI framework.
        services.AddMediator(typeof(AssemblyReference).Assembly);

        // Registers the shared application services with the DI framework.
        services.AddApplication(typeof(PersonService).Assembly);

        // Registers the mappers (IDomainMapper<,> of the application and IPersistanceMapper<,> of the
        // persistence) with the DI framework.
        services.AddMappers(
            Assembly.GetExecutingAssembly(),
            typeof(AssemblyReference).Assembly,
            typeof(PersonMapper).Assembly);

        // Registers the persistence services (IUnitOfWork, IDbSession, repositories and query repositories)
        // with the DI framework.
        services.AddPersistence(
            typeof(PersonRepository).Assembly,
            typeof(GetPersonByIdRepository).Assembly);

        services.AddEndpoints(Assembly.GetExecutingAssembly());

        // Registers the FluentValidation validators of the requests with the DI framework.
        services.AddValidators(Assembly.GetExecutingAssembly());

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

    #endregion
}
