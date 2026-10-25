---
name: new-module
description: Usar SIEMPRE al crear un módulo nuevo (un bounded context nuevo en `Src/Modules/<Módulo>/`), al crear su host (`Src/Apps/<Producto>.<Módulo>.Api.V1`) o al arrancar un proyecto nuevo a partir de esta plantilla. También cuando un caso de uso necesita un bounded context que todavía no existe. Define los proyectos del módulo con sus sufijos (de los que `Directory.Build.targets` infiere la capa), las referencias permitidas entre ellos, cómo agregarlos a la solución `.slnx`, cómo cablear el módulo en el `Program.cs` de un host nuevo o de uno existente, qué se copia de esta plantilla a un proyecto nuevo y la verificación final. Los casos de uso del módulo se agregan después con la skill `new-usecase`.
---

# Módulos nuevos

Un módulo es un bounded context: tiene su propio dominio, sus casos de uso y sus tablas, y no
referencia a otros módulos. Crearlo es raro comparado con agregar un caso de uso, pero las fallas son
del mismo tipo: **si un assembly del módulo no está en el `Program.cs` del host, nada de lo que tiene
se registra, y el build no avisa.**

En los ejemplos, `<Producto>` es el prefijo de la solución (`Records`), `<Módulo>` el bounded context
en plural (`Persons`) y `<Entidad>` su aggregate principal (`Person`). El módulo de referencia es el de
la tabla **Archivos de referencia** del `CLAUDE.md`.

## Antes de empezar

1. **Confirmá con el usuario que es un módulo nuevo y no un aggregate nuevo en un módulo existente.** Si
   el concepto comparte reglas, transacciones o lenguaje con un módulo que ya existe, es un aggregate
   (ver la skill `new-usecase`).
2. **Preguntale dónde se hostea:**
   - **En un host propio** (`Src/Apps/<Producto>.<Módulo>.Api.V1`): es lo que hace la plantilla, un host
     por módulo.
   - **En un host que ya existe:** se suma a su `Program.cs`. Ver
     [Cablear el módulo en un host existente](#cablear-el-módulo-en-un-host-existente).
3. Mostrale la lista de proyectos y archivos que vas a crear, y esperá el OK.

## Proyectos del módulo

Todos van en `Src/Modules/<Módulo>/`. **Respetá los sufijos:** `Directory.Build.targets` infiere la capa
del nombre del proyecto, y con otro nombre el build falla con ARCH001 o deja pasar una referencia que no
corresponde.

| Proyecto | Capa | Referencias a proyectos | Paquetes |
|---|---|---|---|
| `<Producto>.<Módulo>.Domain` | Domain | `BuildingBlocks.Domain` | — |
| `<Producto>.<Módulo>.Dtos` | Dtos | — | — |
| `<Producto>.<Módulo>.Configuration` | Shared | — | — |
| `<Producto>.<Módulo>.Application` | Application | `BuildingBlocks.Application`, `BuildingBlocks.Application.Mappings.Abstractions`, `.Configuration`, `.Domain`, `.Dtos` | `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Options` |
| `<Producto>.<Módulo>.Infra.Persistence.Sql` | Infra (write) | `BuildingBlocks.Application.Persistence.Abstractions`, `BuildingBlocks.Infra.Persistence.Mappings.Abstractions`, `BuildingBlocks.Infra.Persistence`, `My.System.Extensions`, `.Domain` | `Dapper`, `Dapper.Contrib`, `Microsoft.Extensions.DependencyInjection.Abstractions` |
| `<Producto>.<Módulo>.Infra.Queries.Sql` | Infra (read) | `BuildingBlocks.Application.Persistence.Abstractions`, `BuildingBlocks.Infra.Persistence`, `.Application`, `.Dtos` | `Dapper`, `Microsoft.Extensions.DependencyInjection.Abstractions` |
| `<Producto>.<Módulo>.Infra.Configuration.DependencyInjection` | Composition | `.Configuration` | `Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Options`, `Microsoft.Extensions.Options.ConfigurationExtensions`, `Microsoft.Extensions.Options.DataAnnotations` |

- **Copiá cada `.csproj` del proyecto hermano del módulo de referencia** y cambiá solo el nombre del
  módulo en las rutas. Así las versiones de los paquetes quedan iguales (incluidos los overrides de
  seguridad, como `System.Security.Cryptography.Pkcs`). No pongas versiones de memoria.
- La persistencia del write side depende solo del dominio. El read side depende de Application, porque
  implementa los `IQueryRepository<TQuery, TResult>` con las queries de ahí.
- El dominio puede ver los Dtos, pero no `.Configuration`. Si una regla de negocio necesita un valor de
  configuración, Application se lo pasa al dominio como argumento.
- Ningún proyecto del módulo referencia a otro módulo.

### Pasos

1. Creá cada proyecto y borrá el `Class1.cs` que genera la plantilla:

   ```bash
   dotnet new classlib -n <Producto>.<Módulo>.Domain -o Src/Modules/<Módulo>/<Producto>.<Módulo>.Domain
   ```

2. Agregalos a la solución, con la misma carpeta de solución que usan los proyectos hermanos del módulo
   de referencia (los cuatro primeros en `Src/Modules/<Módulo>` y los `Infra.*` en
   `Src/Modules/<Módulo>/Infra`):

   ```bash
   dotnet sln Records.slnx add Src/Modules/<Módulo>/<Producto>.<Módulo>.Domain/<Producto>.<Módulo>.Domain.csproj --solution-folder Src/Modules/<Módulo>
   ```

   ⚠️ No crees un `.sln`: con dos archivos de solución en la raíz, `dotnet build` sin argumento falla
   con MSB1011.
3. En `<Producto>.<Módulo>.Application`, creá el `AssemblyReference.cs` (imitá el del módulo de
   referencia). Es el ancla que usa el host para registrar los handlers del módulo.
4. En `<Producto>.<Módulo>.Configuration`, creá `<Módulo>Settings` (imitá `PersonsSettings`), con su
   `SettingsKey` sin el sufijo `Settings`.
5. En `<Producto>.<Módulo>.Infra.Configuration.DependencyInjection`, creá el `ServiceCollectionExtensions`
   que bindea y valida esos settings (imitá el del módulo de referencia).
   ⚠️ Si el módulo va en un host que ya tiene otro módulo, el método `AddConfiguration` del módulo de
   referencia choca con el del módulo nuevo (CS0121, llamada ambigua). Consultá al usuario antes de
   elegir otro nombre (la propuesta es `Add<Módulo>Configuration`).
6. **Base de datos:** el script `<Producto><Módulo>_Source.sql` en la raíz del repo (imitá
   `RecordsPersons_Source.sql`), con las tablas del módulo. Agregalo a la `.slnx` como `<File>` en la
   carpeta `/Src/Modules/<Módulo>/Infra/DatabaseScripts/`, igual que el del módulo de referencia.
7. **Tests:** el módulo nace sin proyectos de test. Los tests se escriben a pedido, cuando el usuario ya
   consensuó el código (skill `unit-tests`): recién ahí se crean `Tests/Modules/<Módulo>/<Producto>.<Módulo>.TestData`
   (los object mothers) y los `<Proyecto>.UnitTests` que hagan falta. No los crees por tu cuenta.

## Host propio

Creá `Src/Apps/<Producto>.<Módulo>.Api.V1/` (`dotnet new web`) y agregalo a la carpeta de solución
`Src/Apps`.

1. **El `.csproj`:** copiá el del host de referencia y cambiá las referencias a los proyectos del
   módulo. Un host referencia a los siete proyectos del módulo menos `.Domain` y `.Dtos`, que le llegan
   transitivamente, más los `BuildingBlocks.*` de infraestructura (Http, OpenApi, Persistence, Mappings,
   Mediator, Serilog, Application y sus `*.DependencyInjection`).
2. **El `Program.cs`:** copiá el del host de referencia, con **el mismo orden de los `AddXxx`**, y
   reemplazá los tipos ancla por los del módulo nuevo:

   | Llamada | Ancla (el assembly que se escanea) |
   |---|---|
   | `AddMediator(...)` | `typeof(<Módulo>.Application.AssemblyReference).Assembly` |
   | `AddApplication(...)` | Un tipo del Domain (por ejemplo `<Entidad>Service`) |
   | `AddMappers(...)` | El host (`Assembly.GetExecutingAssembly()`), Application y `Infra.Persistence.Sql` |
   | `AddPersistence(...)` | `Infra.Persistence.Sql` (`<Entidad>Repository`) e `Infra.Queries.Sql` (un `<UseCase>Repository`) |
   | `AddEndpoints(...)` y `AddValidators(...)` | El host (`Assembly.GetExecutingAssembly()`) |

   ⚠️ El ancla tiene que ser una clase **pública**: si es `internal`, el host no compila.
3. ⚠️ **`AddOpenApi("v1", ...)` va literal en el `Program.cs`**, nunca en un helper. El source generator
   necesita el literal y resuelve los XML docs según los `ProjectReference` de este proyecto. Si no se
   cumple, Scalar queda sin descripciones y no hay ningún error.
4. **`appsettings.json`:** la sección `SqlServer` (con el `SourceConnectionString` de la base del
   módulo), la sección de `<Módulo>Settings` y Serilog, copiadas del host de referencia.

## Cablear el módulo en un host existente

1. En el `.csproj` del host, agregá las mismas referencias a los proyectos del módulo que en un host
   propio.
2. En el `Program.cs`, sumá los assemblies del módulo nuevo a cada llamada:

   ```csharp
   // AddMediator recibe un solo assembly: se llama una vez por módulo.
   services.AddMediator(typeof(Records.Persons.Application.AssemblyReference).Assembly);
   services.AddMediator(typeof(Records.<Módulo>.Application.AssemblyReference).Assembly);

   // El resto recibe params Assembly[]: se agrega el del módulo nuevo a la lista.
   services.AddApplication(typeof(PersonService).Assembly, typeof(<Entidad>Service).Assembly);
   services.AddMappers(..., typeof(<Entidad>Mapper).Assembly);
   services.AddPersistence(..., typeof(<Entidad>Repository).Assembly, typeof(<UseCase>Repository).Assembly);
   ```

   Llamar dos veces a `AddMediator` es seguro: el mediator se registra una sola vez (`TryAdd`) y cada
   llamada suma los handlers de su assembly. Lo que falla es pasar dos veces **el mismo** assembly: los
   handlers quedan registrados dos veces y `AddMediator` lanza una excepción al arrancar.
3. Si los dos módulos comparten la base, sus tablas no pueden tener el mismo nombre.

## Proyecto nuevo a partir de esta plantilla

Si es el primer módulo de un producto nuevo:

1. **Se copia tal cual:**
   - `Src/Core/My.*` y `Src/BuildingBlocks/*`.
   - `Tests/BuildingBlocks/*`: los tests de los building blocks van con ellos, incluidos los `TODO` de
     los comportamientos que todavía hay que decidir.
   - `Directory.Build.props`, `Directory.Build.targets`, `stylecop.json`, `.editorconfig` (con la
     sección `[Tests/**.cs]`) y `global.json` (fija el SDK y el runner de tests: sin
     `"test": { "runner": "Microsoft.Testing.Platform" }`, `dotnet test` falla en .NET 10).
   - La infraestructura de tests: `Tests/Directory.Build.props`, `dotnet-tools.json` (ReportGenerator)
     y `Tests/coverage.ps1`.
   - `.github/workflows/build.yml`, si el proyecto nuevo también usa GitHub Actions.
   - `.claude/skills/*` y `.claude/commands/handoff*`.
2. **No se copian** `Docs/handoff/*` (es el historial de esta plantilla) ni el módulo Persons, que queda
   solo como ejemplo, ni sus tests (`Tests/Modules/Persons`, `Tests/Apps/Records.Persons.Api.V1.UnitTests`).
3. **La solución** se crea con `dotnet new sln -n <Producto>` (en .NET 10 ya genera `.slnx`), y los
   proyectos se agregan con `dotnet sln add`, incluidos los de `Tests/BuildingBlocks` (en la carpeta de
   solución `Tests/BuildingBlocks`). Si la `.slnx` no se llama `Records.slnx`, actualizá el nombre en
   `Tests/coverage.ps1` y en el workflow.
4. **Se cambia el prefijo** `Records.` por el del producto en los proyectos y los namespaces, también en
   los de `Tests/`. ⚠️ El `Directory.Build.props` expone los tipos `internal` de cada proyecto solo a
   `<Proyecto>.UnitTests` (por convención de nombre): si se renombra un proyecto de `Src/` y no su
   proyecto de test, el test deja de ver los handlers y los validators, y el build falla con errores de
   tipos inaccesibles que no dicen la causa.
5. **Se crea el módulo** con los pasos de arriba y su host propio.
6. **Se crea el `CLAUDE.md` del proyecto nuevo** a partir del de esta plantilla. Se conservan las
   secciones de arquitectura, decisiones, convenciones, trampas y flujo de trabajo, y se actualizan los
   comandos (el nombre de la `.slnx` y del host) y las rutas de los **Archivos de referencia**, que
   apuntan a los archivos del módulo nuevo a medida que existan.

## Verificación final

1. **Build:** `dotnet build <Producto>.slnx` con 0 warnings. Un ARCH001 es un sufijo mal puesto; un
   ARCH002, una referencia prohibida. Se corrige el nombre o la referencia, nunca la validación.
2. **La capa de cada proyecto nuevo** es la que corresponde según la tabla de
   [Proyectos del módulo](#proyectos-del-módulo):

   ```bash
   dotnet msbuild Src/Modules/<Módulo>/<Producto>.<Módulo>.Domain/<Producto>.<Módulo>.Domain.csproj -getProperty:ArchLayer
   ```

3. **La solución tiene todos los proyectos**, en la carpeta correcta: `dotnet sln <Producto>.slnx list`.
4. **El host arranca** en Development (ver el paso 3 de la verificación de `new-usecase`): el contenedor
   de DI valida el grafo al arrancar, así que un assembly que falta en el `Program.cs` aparece como
   "Some services are not able to be constructed" en cuanto haya un handler que dependa de algo de ese
   assembly. Con el módulo vacío no hay nada que validar: la prueba real es el primer caso de uso.
5. **Si es un proyecto nuevo, los tests copiados pasan:** `dotnet test --solution <Producto>.slnx` corre
   los de `Tests/BuildingBlocks` en verde.
6. **Seguí con el primer caso de uso** con la skill `new-usecase`, que termina con su propia
   verificación (incluida la ruta en `/openapi/v1.json`).
