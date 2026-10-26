# Handoff: tests-y-skill-de-tests

## Objetivo

Sumar unit tests al repo y dejar escrito **cómo se escriben**, para que este proyecto sirva de
plantilla también para los tests del negocio de otros proyectos. El foco son los tests "de negocio"
(dominio, command handlers, mappers y validators), no los técnicos de BuildingBlocks.

En el camino, los tests encontraron varios hallazgos en `Src/` (igualdades, límites, `Money`, código
muerto) que se resolvieron uno por uno, consultando cada decisión.

## Qué se hizo

### Infraestructura de tests

- **xUnit v3 sobre Microsoft Testing Platform**, con AwesomeAssertions y Moq. El runner está en
  `global.json` (`"test": {"runner": "Microsoft.Testing.Platform"}`), por eso se corre con
  `dotnet test --solution Records.slnx` (el `--solution` es obligatorio).
- **`Tests/Directory.Build.props`:** aplica xUnit, Moq, AwesomeAssertions, la cobertura,
  `OutputType Exe` e `IsTestProject` **solo a los proyectos que terminan en `.UnitTests`**. Un
  proyecto `TestData` es una librería común.
- **`Directory.Build.props` de la raíz:** `<InternalsVisibleTo Include="$(AssemblyName).UnitTests" />`,
  por convención de nombre. Si se renombra el prefijo de un proyecto, hay que renombrar también su
  proyecto de test.
- **`Tests/` tiene la misma estructura que `Src/`**, con un proyecto de test por proyecto testeado.
- **La capa `Test`** la infiere `Directory.Build.targets` por la carpeta, y puede referenciar todo.

### Cobertura (local, a pedido)

- `Microsoft.Testing.Extensions.CodeCoverage` + **ReportGenerator** como tool local
  (`dotnet-tools.json`).
- `Tests/coverage.ps1` deja todo en `Tests/TestResults/coverage-report/` (ignorada por git):
  `Summary.txt` y `Cobertura.xml` (mergeado y filtrado) para el agente, `index.html` para la persona.
- **Por qué ReportGenerator y no solo el de VS:** el de VS es para verlo en el IDE. ReportGenerator
  mergea los resultados de todos los proyectos de test, filtra por ensamblado y genera un texto
  (`Summary.txt`) que el agente puede leer sin gastar contexto en el XML entero.
- **No corre en el CI**, y se corre solo cuando el usuario lo pide.

### Tests escritos (279 en total, CI en verde)

| Proyecto | Qué cubre |
|---|---|
| `Tests/Modules/Persons/Records.Persons.Domain.UnitTests` | El aggregate `Person`, las entidades hijas, los value objects, `Gender` y `PersonService`. |
| `Tests/Modules/Persons/Records.Persons.TestData` | Los object mothers compartidos (`PersonMother`, `PersonDtoMother`, `public static`). |
| `Tests/BuildingBlocks/BuildingBlocks.Domain.UnitTests` | `Email`, `PhoneNumber`, `LatLng`, `ValueObject`, `Entity`, `AggregateRoot`, `Enumeration`, `Error` y las excepciones. Los test doubles están en `TestData/` (`TestEntity`, `TestAggregate`, etc.). |
| `Tests/Modules/Persons/Records.Persons.Application.UnitTests` | Los command handlers de Create, Update y Delete, y el mapper dominio → DTO. |
| `Tests/Modules/Persons/Records.Persons.Infra.Persistence.Sql.UnitTests` | El mapper dominio ↔ data model (round trip). |
| `Tests/Apps/Records.Persons.Api.V1.UnitTests` | Los validators de FluentValidation. |

### Skills

- **`unit-tests` (nueva):** cuándo se escriben, dónde van, qué se testea según el tipo, el naming
  `Method_WhenCondition_ShouldExpectedResult` (con `WhenCalled` si no hay condición), AAA con sus
  comentarios, excepciones por `Error.Code`, object mothers, command handlers, mappers, validators,
  qué hacer cuando un test descubre algo raro y cómo se lee la cobertura.
- **`new-usecase`:** la verificación incluye `dotnet test`.
- **`new-module`:** el módulo nuevo nace sin proyectos de test, y la sección "Proyecto nuevo" copia la
  infraestructura de tests (`Tests/BuildingBlocks/*`, `global.json`, `Tests/Directory.Build.props`,
  `dotnet-tools.json`, `Tests/coverage.ps1` y el workflow), pero no los tests de Persons.
- **`code-comments`:** el ejemplo de `TODO` ahora es el del enum `Gender` en el DTO.
- **`CLAUDE.md`:** los comandos de test y cobertura, y las reglas de cuándo se escriben y se corren.

## Decisiones tomadas y por qué

1. **Los tests nuevos se escriben solo a pedido, con el código ya consensuado.** Si se escriben junto
   con un código que después cambia en la revisión, hay que rehacer las dos cosas. Y si el agente
   testea lo que acaba de escribir, los tests confirman lo que hizo, no lo que debería hacer. Los
   tests existentes, en cambio, se corren siempre que se cambia `Src/`.
2. **Cuando un test descubre algo raro, no se ajusta el test ni se corrige `Src/` por iniciativa
   propia:** el test documenta el comportamiento de hoy con un `TODO` y se avisa. Así se encontraron
   todos los hallazgos de abajo.
3. **Los tests técnicos de BuildingBlocks (`GlobalExceptionHandler`, `Mediator`,
   `DomainEventPublisher`) quedan para otro momento.** El usuario prefiere que el repo sea la
   plantilla de cómo se testea el negocio.
4. **Patrones por tipo de clase:**
   - **Command handlers:** un `CommandHandlerTestContext` registra el orden de las llamadas con
     callbacks de Moq (`Calls`, `PublishedEvents`), para verificar el flujo cargar → comportamiento →
     transacción → eventos después del commit.
   - **Mappers:** un round trip con `BeEquivalentTo(..., options => options.ComparingByMembers<T>())`,
     porque las entidades redefinen `Equals` por ID.
   - **Validators:** `TestValidate` con `ShouldHaveValidationErrorFor`. El límite que pasa y el que
     falla van en tests separados, nunca en un `[Theory]` con un `bool` de resultado esperado.
5. **Hallazgos resueltos en `Src/`:**
   - **Igualdad:** los value objects y las entidades comparan el tipo (una dirección y un bien con el
     mismo ID daban iguales con `==`).
   - **Email:** la misma regex pragmática en el dominio y en el validator
     (`^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$`), con un test de consistencia entre las dos. Se descartó
     compartir la regex desde un lugar común: el validator de la API no debe depender del dominio, y
     las otras opciones (appsettings, un `Constants` en Core) no convencieron.
   - **Largos:** el teléfono es 30 y el país 60 en el dominio, el validator y la columna. Se modificó
     el script SQL directamente, sin `ALTER`, porque todavía no hay una base creada.
   - **`Money` → `PersonalAssetValue`:** de 0 a 999.999.999,99 y con 2 decimales como máximo. Un
     valor con más decimales se rechaza (no se redondea, porque sería un cambio invisible). La
     columna es `decimal(11, 2)`, y el validator usa `PrecisionScale(11, 2, true)`. `Money` era
     demasiado genérico.
   - **Entidades transient:** una entidad sin ID todavía (ID 0 o `Guid.Empty`) solo es igual a sí
     misma, y su `GetHashCode` es el de la referencia (`Entity.IsTransient()`). Antes, dos bienes
     nuevos daban iguales, y un `Contains`, un `Remove` o un `HashSet` los trataba como uno solo.
     **No reemplaza la relectura** de la persona en los handlers de Create y Update: esa sigue
     haciendo falta porque el ID generado por el `IDENTITY` no vuelve al aggregate (Dapper.Contrib
     lo deja en el data model, y el `Id` es `init`).
   - **Código muerto:** se borraron `DomainErrors.Person.InvalidCountry` y la tabla `Countries` del
     script, restos de un diseño con el país como entidad.

## Qué quedó pendiente

- **`PersonalAssetCreatedEvent` y la línea comentada en `PersonalAsset.cs:45`:** el usuario decidió
  dejarlos como están por ahora.
- **Los tests técnicos de BuildingBlocks** (`GlobalExceptionHandler`, `Mediator`,
  `DomainEventPublisher`): necesitan un patrón nuevo (los de infraestructura no siguen el de los
  tests del negocio) y hay que acordarlo antes de escribirlos.
- **No hay tests de las queries ni de los repositorios:** son SQL contra la base, así que serían
  tests de integración, no unit tests.
- **Sin patrón todavía para query handlers, event handlers y endpoints:** la skill `unit-tests` dice
  que no se inventa, y que se propone y se acuerda antes de escribirlos.
- **Este handoff no está en la tabla de handoffs de `CLAUDE.md`.**

## Próximos pasos concretos

1. Agregar este handoff a la tabla de `CLAUDE.md`.
2. Si se retoman los tests técnicos: proponer el patrón (por ejemplo, para `GlobalExceptionHandler`,
   un `DefaultHttpContext` y leer el `ErrorResponse` del body), acordarlo, escribir los tests y sumar
   la sección a la skill.
3. Decidir qué pasa con `PersonalAssetCreatedEvent` (usarlo al agregar un bien, o borrarlo).

## Gotchas

- **`dotnet test` sin `--solution` falla** con Microsoft Testing Platform.
- **`SetupSequence` seguido de otro `Setup` sobre el mismo método:** el segundo pisa al primero. Para
  devolver valores distintos en llamadas sucesivas se usa una `Queue` con
  `ReturnsAsync(() => queue.Dequeue())`.
- **CA1508 en los tests de `==` con `null`:** el analyzer asume el `==` estándar y marca la condición
  como muerta. Se suprime con un `SuppressMessage` y una justificación.
- **CA1826:** con colecciones indexables se usa `[0]` y `[^1]` en lugar de `First()` y `Last()`.
- **`BeEquivalentTo` con entidades:** sin `ComparingByMembers<T>()`, AwesomeAssertions usa el `Equals`
  por ID y el round trip no compara nada.
- **Al hacer una mutación para verificar un test**, chequeá que el reemplazo realmente se aplicó: un
  `sed` encontró el texto en un comentario y no en el código, y los tests pasaban sin probar nada.
- **Line endings:** `sed` en Git Bash deja LF en algunos archivos. Git los normaliza (lo fuerza
  `.gitattributes` para los `.cs`), y el build y el CI pasan igual.
- **`TODO.md` lo modifica el usuario** y no se commitea salvo que lo pida.
- **CI:** el runner `ubuntu-latest` pasa a Ubuntu 26 desde octubre de 2026. Si algún día rompe, se
  fija la versión en `.github/workflows/build.yml`.
