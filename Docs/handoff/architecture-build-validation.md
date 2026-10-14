# Handoff: architecture-build-validation

## Objetivo

Impedir que un proyecto referencie a otro violando la arquitectura (Clean Architecture + DDD), p.ej. que el Domain
referencie al Application. Antes de elegir la herramienta se definió la jerarquía de capas y después se implementó
la validación en el build, sin paquetes, con un target propio en `Directory.Build.targets`.

## Qué se hizo

Todo en `feature/mediator`.

### Jerarquía acordada

| Capa | Proyectos (convención de nombre) | Puede referenciar |
|---|---|---|
| Core | `My.*` | Core |
| Dtos | `*.Dtos` | Core, Dtos |
| Shared | `*.Contracts`, `*.Messaging`, `*.Configuration` | Core, Dtos, Shared |
| Domain | `*.Domain`, `*.Domain.*` | Core, Dtos, Domain |
| Application | `*.Application`, `*.Application.*` | Core, Dtos, Shared, Domain, Application |
| Infra | `*.Infra.*` | Core, Dtos, Shared, Domain, Application, Infra |
| Composition | `*.DependencyInjection` | Todo menos Composition y Host (solo Host la referencia) |
| Host | proyectos bajo `Src/Apps/` | Todas |

### Archivos

| Archivo | Cambio | Por qué |
|---|---|---|
| `Directory.Build.targets` (nuevo) | Infiere `ArchLayer` por nombre/carpeta del proyecto, define la matriz `_ArchAllowedLayers`, target `GetArchLayer` (devuelve la capa a quien referencia) y target `ValidateArchitecture` (`BeforeTargets="ResolveProjectReferences"`) que emite `ARCH001` (capa no inferible) y `ARCH002` (referencia prohibida). Cabecera con la tabla y links a Microsoft Learn. | Falla en el build (CLI y VS) en cuanto alguien agrega la referencia; no requiere proyecto de tests. |
| `BuildingBlocks.Infra.Persistence.Abstractions` → `BuildingBlocks.Application.Persistence.Abstractions` | Rename de carpeta, csproj y namespace (`IDbSession`, `IUnitOfWork`, `IQueryRepository`, `IEntityAuditable`, `IEntitySoftDeletable`). Actualizados `Records.sln`, 5 csproj y los usings de los `.cs` (reordenados alfabéticamente). | Era la única violación: son puertos que usa Application, con nombre de Infra. |
| `BuildingBlocks.Mediator.Abstractions.csproj` | `<ArchLayer>Application</ArchLayer>` | Es un puerto; el nombre no permite inferirlo. |
| `BuildingBlocks.Mediator.csproj` | `<ArchLayer>Infra</ArchLayer>` | Es la implementación. |
| `Records.sln` | `Directory.Build.targets` agregado a `Solution Items` (lo hizo el usuario). | Visible desde VS. |

### Decisiones (discutidas con el usuario)

- **Mecanismo: target MSBuild propio** en vez de tests de arquitectura (ArchUnitNET/NetArchTest) o NsDepCop. No hay
  proyecto de tests y la convención de nombres ya alcanza para clasificar. Los tests a nivel tipo quedan como
  complemento futuro.
- **`ArchLayer` y `ArchValidationEnabled` son propiedades propias del repo**, no del SDK. Documentado en la cabecera.
- **Domain puede ver los Dtos** (para algún read que no pase por una entidad), pero **no `*.Configuration`**: una
  regla de negocio parametrizable se modela en el dominio (argumento/value object o puerto `I...Policy`) y
  Application le pasa el valor desde `IOptions<...Settings>`. `PersonsSettings` es un contrato con el
  `appsettings.json` (`SettingsKey`, setters, `[Required]`), no con el negocio.
- **Dtos es su propia capa** (separada de Shared) para que Domain la vea sin ver Contracts/Messaging/Configuration.
- **`*.DependencyInjection` solo desde Host.** Hoy ningún otro proyecto los referencia.
- **Solo se validan referencias directas**: con capas ordenadas, las transitivas quedan cubiertas.
- **Convención + override**: la capa se infiere del nombre (orden: Host → Composition → Infra → Application →
  Domain → Dtos → Shared → Core) y el csproj puede fijarla con `<ArchLayer>`.

### Verificación hecha

- `dotnet build Records.sln` sin errores (solo los warnings NU1902/NU1903 de `System.Security.Cryptography.Pkcs`, previos).
- `dotnet msbuild <csproj> -getProperty:ArchLayer` en los 30 proyectos: todas las capas correctas.
- Pruebas negativas (temporales, revertidas): Domain → `Records.Persons.Configuration` y Domain →
  `BuildingBlocks.Application.Mappings.Abstractions` ⇒ dos `ARCH002`; Domain → `Records.Persons.Dtos` permitido;
  `Records.Persons.Infra.Queries.Sql` → `BuildingBlocks.Infra.Http.DependencyInjection` ⇒ `ARCH002`.

## Pendiente

- No se verificó desde Visual Studio que el `ARCH002` aparezca en el Error List (solo con `dotnet build`).
- Reglas que quedaron fuera de alcance: paquetes NuGet prohibidos por capa (Dapper/SqlClient/AspNetCore/Serilog en
  Domain/Application), aislamiento entre módulos (`Records.X.*` ↛ `Records.Y.*`), `*.Abstractions` sin
  referencias a implementaciones, tests de arquitectura a nivel tipo.

## Próximos pasos

1. Compilar desde VS con una referencia prohibida y confirmar el error en el Error List.
2. Si se quieren las reglas extra: agregar `PackageReference` prohibidos por capa en `ValidateArchitecture`
   (mismo patrón: item con las prohibidas + `Error` con `ContinueOnError="ErrorAndContinue"`).
3. Al crear el primer proyecto de tests: declararle `<ArchLayer>Host</ArchLayer>` (o
   `<ArchValidationEnabled>false</ArchValidationEnabled>`), si no da `ARCH001`.

## Gotchas

- **`Error` corta en el primer lote**: con batching sobre `%(_ArchViolation...)` solo se reportaba la primera
  violación. Se resolvió con `ContinueOnError="ErrorAndContinue"`.
- **La validación no corre en design-time builds** (`DesignTimeBuild == true`) para no meter ruido en VS; el error
  aparece al compilar.
- **Probar con un ciclo no sirve**: Domain → Application da `MSB4006` (circular dependency en
  `_GenerateRestoreProjectPathWalk`) en el restore, antes de llegar al target. Para probar, usar referencias sin ciclo.
- **`sed` en Git Bash** se come las `\` de las rutas en `ProjectReference`; usar `/`.
- **Rename + usings**: al cambiar `Infra` por `Application` en el namespace, los `using` quedan fuera de orden
  (SA1210). Ojo con ordenar con `sort` simple: pone `Dapper.Contrib.Extensions` antes de `Dapper`.
- Si tras el rename el IDE muestra errores con build limpio → limpiar caché de ReSharper.
