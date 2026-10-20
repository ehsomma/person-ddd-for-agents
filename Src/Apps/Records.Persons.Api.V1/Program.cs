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
/// Punto de entrada de la aplicación.
/// </summary>
internal sealed class Program
{
    #region Public methods

    /// <summary>
    /// Crea el host de la aplicación web: registra los servicios, arma el pipeline de middlewares y
    /// arranca el servidor, que queda escuchando los requests HTTP.
    /// </summary>
    /// <param name="args">Los argumentos de línea de comandos con los que se ejecutó la aplicación.</param>
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        IServiceCollection services = builder.Services;

        // Configura y registra Serilog como logger.
        builder.AddSerilogCustom();

        // Registra las configuraciones (settings) en el contenedor de DI.
        services.AddConfiguration(builder.Configuration);

        // Registra el GlobalExceptionHandler, que convierte cualquier excepción en un ErrorResponse
        // con el status HTTP que corresponde a su ErrorType.
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Hace que minimal APIs lance BadHttpRequestException ante un request mal formado (body
        // faltante, JSON inválido, header requerido ausente, etc.) en todos los ambientes, para que
        // el GlobalExceptionHandler devuelva el ErrorResponse. Por defecto solo lo hace en
        // Development; en el resto devuelve un 400 vacío.
        services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

        // Registra los servicios del mediator (commands y queries) en el contenedor de DI.
        services.AddMediator(typeof(AssemblyReference).Assembly);

        // Registra los servicios compartidos de application y los servicios de dominio en el
        // contenedor de DI.
        services.AddApplication(typeof(PersonService).Assembly);

        // Registra los mappers (IDomainMapper<,> de application e IPersistenceMapper<,> de
        // persistencia) en el contenedor de DI.
        services.AddMappers(
            Assembly.GetExecutingAssembly(),
            typeof(AssemblyReference).Assembly,
            typeof(PersonMapper).Assembly);

        // Registra los servicios de persistencia (IUnitOfWork, IDbSession, repositorios y query
        // repositories) en el contenedor de DI.
        services.AddPersistence(
            typeof(PersonRepository).Assembly,
            typeof(GetPersonByIdRepository).Assembly);

        // Registra los endpoints (las clases que implementan IEndpoint).
        services.AddEndpoints(Assembly.GetExecutingAssembly());

        // Registra los validators de FluentValidation de los requests en el contenedor de DI.
        services.AddValidators(Assembly.GetExecutingAssembly());

        // Registra los servicios de autorización.
        services.AddAuthorization();

        // Configura y registra la generación del documento OpenAPI.
        //
        // NOTE: AddOpenApi se llama acá (con "v1" como literal) y no en un helper de la librería
        // compartida porque el source generator que lee los comentarios XML de los DTOs (<summary>,
        // <example>, etc.) resuelve esos comentarios según las ProjectReference del proyecto donde
        // está el call site. Ver el comentario en ConfigureOpenApiCustom para más detalle.
        const string description =
            "Proyecto modelo base desarrollado en C# sobre .NET 10, " +
            "implementado bajo el enfoque de Domain - Driven Design(DDD) y el patrón CQRS, " +
            "estructurado mediante Clean Architecture y aplicando estándares de codificación, naming, " +
            "arquitectura de directorios y documentación de código.";
        services.AddOpenApi("v1", options => options.ConfigureOpenApiCustom(
            title: "Persons - Proyecto modelo",
            version: "v1",
            description: description));

        // Habilita el API explorer para los endpoints.
        services.AddEndpointsApiExplorer();

        WebApplication app = builder.Build();

        // Builder vacío: nuestro handler escribe la respuesta. Sin esto y al no usar el estándar
        // `ProblemDetails` para devolver errores, falla al arrancar.
        app.UseExceptionHandler(_ => { });

        // Configura el pipeline de requests HTTP.
        app.MapOpenApi();

        // Expone el documento OpenAPI (JSON) en una UI.
        // NOTE: Para que al ejecutar el api abra directamente la UI de Scalar, agregar lo siguiente
        // en el archivo launchSettings.json:
        //
        // "launchBrowser": true,
        // "launchUrl": "v1/docs",
        ////app.MapScalarApiReference(); // https://localhost:xxxx/scalar/v1
        app.MapScalarApiReference("/v1/docs"); // https://localhost:xxxx/v1/docs

        app.UseHttpsRedirection();

        app.UseAuthorization();

        ////app.UseSerilogRequestLogging(); // Se descomenta para loguear cada request.

        // Mapea los endpoints.
        app.MapEndpoints();

        app.Run();
    }

    #endregion
}
