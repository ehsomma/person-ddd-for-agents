---
name: unit-tests
description: Usar SIEMPRE al escribir, revisar o modificar unit tests en este repositorio (todo lo que está bajo `Tests/`), al crear un proyecto de test nuevo, o cuando se pide analizar o verificar la cobertura de código ("revisá el coverage", "qué queda sin cubrir"). Define cuándo se escriben los tests (a pedido, con el código ya consensuado), dónde van, qué se testea según el tipo (aggregate, entidad, value object, enumeración, servicio de dominio), cómo se escriben (xUnit v3, AwesomeAssertions, Moq, naming Method_WhenCondition_ShouldExpectedResult, AAA, object mother), qué hacer cuando un test descubre un comportamiento dudoso y cómo se interpreta la cobertura. Hoy cubre la capa Domain; las otras capas se agregan cuando se escriban sus primeros tests. Complementa a `team-standards` y `code-comments`, que aplican igual en los tests.
---

# Unit tests

Cómo se escriben los tests de este repo. Los ejemplos salen de
`Tests/Modules/Persons/Records.Persons.Domain.UnitTests/`: ante la duda, se imita ese proyecto.

Es un documento vivo, igual que `team-standards`. **Hoy cubre solo la capa Domain.** Para otra capa
(Application, validators, infra) todavía no hay un patrón acordado: no lo inventes. Frená, proponé
cómo encararlo, y cuando el usuario lo apruebe, agregá acá la sección de esa capa.

## Cuándo se escriben

- **Los tests nuevos se escriben solo a pedido**, cuando el usuario ya revisó y consensuó el código.
  No se agregan por iniciativa propia al crear o modificar código.
- **Los tests existentes se corren siempre** que se cambia código de `Src/`
  (`dotnet test --solution Records.slnx`), y el resultado va en el reporte. Si un cambio rompe un
  test, no se "arregla" el test para que pase: se avisa (ver
  [Cuando un test descubre algo raro](#cuando-un-test-descubre-algo-raro)).

**Por qué:** un test fija comportamiento. Si se escribe junto con un código que después cambia en la
revisión, hay que rehacer las dos cosas. Y si el mismo agente testea el código que acaba de escribir,
los tests confirman lo que hizo, no lo que debería hacer: un error de interpretación queda congelado
como correcto. Con el código consensuado, los tests salen de lo acordado y no de la implementación.

## Dónde van

- `Tests/` tiene la misma estructura de carpetas que `Src/`, y hay **un proyecto de test por proyecto
  testeado**, con el sufijo `.UnitTests`:
  `Src/Modules/Persons/Records.Persons.Domain` → `Tests/Modules/Persons/Records.Persons.Domain.UnitTests`.
- Dentro del proyecto, cada clase de test va en la misma carpeta relativa que la clase testeada y se
  llama `<Clase>Tests`: `PersonAggregate/Models/Person.cs` → `PersonAggregate/Models/PersonTests.cs`.
- Los object mothers van en la carpeta `TestData/` del proyecto.

### Proyecto de test nuevo

1. El `.csproj` es mínimo: el resto (xUnit, AwesomeAssertions, Moq, la cobertura, `OutputType Exe`)
   lo hereda de `Tests/Directory.Build.props`. Imitá
   `Tests/Modules/Persons/Records.Persons.Domain.UnitTests/Records.Persons.Domain.UnitTests.csproj`:
   `TargetFramework`, `ImplicitUsings`, `Nullable` y el `ProjectReference` al proyecto testeado.
2. No se declara `<ArchLayer>`: `Directory.Build.targets` infiere la capa `Test` por la carpeta
   `Tests/`.
3. Se agrega a la solución en la carpeta espejo:
   `dotnet sln Records.slnx add <ruta>.csproj --solution-folder Tests/Modules/<Módulo>`.

## Qué se testea según el tipo

| Tipo | Qué se testea | Ejemplo |
|---|---|---|
| Aggregate | `Create`: las propiedades, el evento de creación, las fechas de auditoría y un test por cada dato obligatorio en `null`. `Load`: que no registra eventos y que conserva lo que recibe. Cada método de comportamiento: el cambio de estado, el evento que registra y sus casos borde. | `PersonTests` |
| Entidad hija | `Create` con Id `0` y `Load` con el Id recibido, y los datos obligatorios. | `AddressTests`, `PersonalAssetTests` |
| Value object | Un valor válido, el largo máximo exacto (pasa), el máximo + 1 (falla), `null` y vacío según sea obligatorio u opcional, y la igualdad por valor en uno representativo. | `FullNameTests`, `StateTests`, `CountryNameTests` |
| Enumeración | La conversión desde el nombre o el valor, y sus casos borde (mayúsculas, inexistente). | `GenderTests` |
| Servicio de dominio | Con el repositorio mockeado: el resultado, el error cuando no existe, que el `CancellationToken` llega al repositorio y el `ArgumentNullException` del constructor. | `PersonServiceTests` |

**No se testea:**

- Los getters y setters triviales, ni el framework (que xUnit, Dapper o el contenedor de DI funcionen).
- Los métodos privados directamente: se cubren a través del método público que los usa.
- El texto de los mensajes de error: se compara el código (ver más abajo).
- El código muerto: si algo no se usa, se avisa (ver [Cobertura](#cobertura)), no se le escribe un test.

## Cómo se escriben

### Naming: `Method_WhenCondition_ShouldExpectedResult`

- `Method` es el método testeado (`Create`, `Update`, `Build`, `GetByIdAsync`, `Constructor`).
- `WhenCondition` describe el escenario. Si el método no tiene una condición que distinga al escenario,
  se escribe `WhenCalled` (no se omite).
- `ShouldExpectedResult` describe el resultado observable.

```csharp
Create_WhenFullNameIsNull_ShouldThrowDomainValidationException()
Update_WhenAddressComesWithoutId_ShouldKeepCurrentAddressId()
Delete_WhenCalled_ShouldRegisterPersonDeletedEvent()
GetByIdAsync_WhenPersonDoesNotExist_ShouldThrowNotFound()
```

El `.editorconfig` apaga CA1707 en `Tests/` justamente para permitir los `_`.

### Un comportamiento por test

Cada test verifica una sola cosa: las propiedades de `Create` en un test y su evento en otro. Si falla,
el nombre ya dice qué se rompió. Varios `Should()` en el mismo test están bien si verifican el mismo
comportamiento (todas las propiedades que asigna `Create`).

### AAA con sus comentarios

Todo test tiene los tres comentarios `// Arrange.`, `// Act.` y `// Assert.`, en ese orden y con punto.
El `// Arrange.` va aunque quede vacío, así todos los tests se leen igual.

```csharp
[Fact]
public void Load_WhenDataIsValid_ShouldNotRegisterDomainEvents()
{
    // Arrange.

    // Act.
    Person person = PersonMother.ExistingPerson(Guid.NewGuid());

    // Assert.
    person.PullDomainEvents().Should().BeEmpty();
}
```

### Excepciones

El `Act` se guarda en un `Action` (o un `Func<Task>` si es async) y el `Assert` verifica sobre él:

```csharp
// Act.
Action act = () => FullName.Build(value);

// Assert.
act.Should().Throw<DomainValidationException>();
```

Una `DomainException` se compara **por `Error.Code`, nunca por el mensaje**: el código es el contrato
(viaja en el `ErrorResponse`) y el mensaje puede cambiar.

```csharp
await act.Should().ThrowAsync<DomainException>()
    .Where(exception => exception.Error.Code == DomainErrors.Person.NotFound.Code);
```

### Domain events

Se sacan con `PullDomainEvents()` y se verifica el tipo y el contenido:

```csharp
IReadOnlyCollection<IDomainEvent> domainEvents = person.PullDomainEvents();
domainEvents.Should().ContainSingle()
    .Which.Should().BeOfType<PersonDeletedEvent>()
    .Which.Person.Should().BeSameAs(person);
```

### Object mother

Los objetos válidos del aggregate salen de un object mother (`TestData/<Aggregate>Mother.cs`,
`internal static`). Cada test parte de ahí y cambia solo el dato que le interesa, así el `Arrange`
queda corto y se ve qué distingue al escenario.

- `New*` usa `Create` (objeto nuevo, con el evento de creación registrado, Id `0` en las entidades
  hijas).
- `Existing*` usa `Load` (objeto ya guardado, tal como lo devuelve el repositorio, sin eventos).
- Los datos que el test necesita controlar entran como parámetros opcionales
  (`ExistingPerson(id, addressId: 7)`).
- Las fechas fijas son campos `public static readonly` del mother, para poder compararlas.

Si el test necesita un dato inválido o un `null`, no se usa el mother: se llama a `Create` o `Build`
directamente con ese dato (ver `Create_WhenAddressIsNull_ShouldThrowDomainValidationException`).

### Límites de los value objects

El largo máximo va en una constante `MaxLength` con un `NOTE` que la ata a la columna de la base, y
los valores se arman con `new string('a', MaxLength)` y `MaxLength + 1`:

```csharp
// NOTE: Tiene que coincidir con el largo de la columna en la base (RecordsPersons_Source.sql).
// Si este test falla porque cambió el límite, revisá también la columna.
private const int MaxLength = 62;
```

### `[Fact]` y `[Theory]`

`[Theory]` con `[InlineData]` cuando el mismo comportamiento se prueba con varios valores (`null` y
vacío; los tres valores de una enumeración). Si los valores llevan a resultados distintos, son tests
distintos.

### Mocks (Moq)

- El mock es un field `private readonly` de la clase, creado en la declaración. xUnit crea una
  instancia de la clase por test, así que cada test arranca con un mock limpio.
- La clase testeada se crea en el `Arrange` de cada test, después de los `Setup`.
- Se mockean solo las dependencias (repositorios), nunca el dominio: los aggregates y value objects
  se crean de verdad con el mother.
- Para verificar que un argumento llega a la dependencia (el `CancellationToken`), se usa `Verify`
  con el valor exacto y `Times.Once`.

### Async

El test es `async Task`, y el `CancellationToken` que se pasa es `TestContext.Current.CancellationToken`
(xUnit v3), salvo que el test necesite uno propio para verificarlo.

### Estructura de la clase

- `public sealed class <Clase>Tests` (xUnit exige clases públicas: por eso CA1515 está apagada en
  `Tests/`).
- Aplican las reglas de `team-standards`: regions (`Declarations` para las constantes y los mocks,
  `Public methods` para los tests), `new T()` explícito, tipo explícito en lugar de `var`, `return` de
  una variable y la línea en blanco antes del `return`.

### Comentarios

Aplica `code-comments`, con el español de `Src/Modules`. Además:

- La clase lleva un `<summary>` que dice qué se testea y, en los value objects, sus reglas
  ("obligatorio, de 1 a 62 caracteres").
- Los métodos de test no llevan XML docs: el nombre ya documenta el escenario (SA1600 y CS1591 están
  apagadas en `Tests/`).
- Todo escenario que no se entiende con el nombre lleva un comentario en el `Arrange` o el `Assert`
  que explica el porqué, con ejemplos alineados si ayudan (ver
  `Update_WhenEditedDataHasNewAndExistingPersonalAssets_ShouldAddOnlyTheNewOnes`).

## Cuando un test descubre algo raro

Si al escribir un test aparece un comportamiento que parece un error (un límite que no coincide con la
base, un dato inválido que se acepta, un test existente que falla después de un cambio):

1. **No se ajusta el test en silencio** para que pase, ni se corrige el código de `Src/` por iniciativa
   propia.
2. El test documenta el comportamiento de hoy, con un `TODO:` que explica el problema y qué hay que
   decidir.
3. Se avisa en el reporte, para que el usuario decida.

```csharp
// TODO: El value object acepta hasta 160 caracteres, pero la columna Addresses.Country es
// nvarchar(60) (RecordsPersons_Source.sql): un país de 61 a 160 caracteres pasa el dominio y falla
// al insertar. Hay que decidir cuál es el límite correcto y alinear el value object, la columna y
// este test.
private const int MaxLength = 160;
```

**Por qué:** encontrar estas cosas es para lo que sirven los tests. Si el agente las tapa ajustando el
test, o las "arregla" sin consultar, la decisión de negocio la toma el agente sin que nadie se entere.

## Cobertura

Se corre **solo cuando el usuario la pide**:

```powershell
./Tests/coverage.ps1 -Assemblies "+Records.Persons.Domain"
```

Todo queda en `Tests/TestResults/coverage-report/`, que está ignorada por git:

1. Se lee primero `Summary.txt`: los porcentajes por ensamblado y por clase.
2. Solo para las clases con huecos, se lee `Cobertura.xml` de la misma carpeta: cada línea y branch con
   sus hits (`hits="0"` es sin cubrir), ya sumados de todos los proyectos de test y filtrados por
   `-Assemblies`. No se lee el XML entero: crece con el código y gasta contexto.
3. `index.html` es para la persona; el agente no lo necesita.

Cada hueco se busca en el código (con un `grep` de sus usos) y se clasifica:

| Hueco | Acción sugerida |
|---|---|
| Una regla de negocio o un caso borde sin test | Proponer el test que falta. |
| Código muerto (nadie lo usa) | Señalarlo y proponer borrarlo. No se borra sin confirmación. |
| Código que no vale la pena testear (un getter, un mapper trivial) | Dejarlo y decir por qué. |

No se persigue el 100%: un test que solo construye un objeto para sumar cobertura, sin verificar
comportamiento, no se escribe. El reporte también menciona lo que la cobertura no ve: código cubierto
cuyo comportamiento es dudoso (ver [Cuando un test descubre algo raro](#cuando-un-test-descubre-algo-raro)).

## Verificación

1. `dotnet build Records.slnx` con 0 warnings.
2. `dotnet test --solution Records.slnx`, con todos los tests en verde (el número de tests va en el
   reporte).

## Checklist antes de cerrar

- [ ] Los tests nuevos se pidieron y el código que testean ya estaba consensuado.
- [ ] El proyecto y las clases de test están en la carpeta espejo de `Src/`, con el sufijo correcto.
- [ ] Los nombres siguen `Method_WhenCondition_ShouldExpectedResult` (con `WhenCalled` si no hay
      condición).
- [ ] Todos los tests tienen `// Arrange.`, `// Act.` y `// Assert.`, y verifican un solo
      comportamiento.
- [ ] Las `DomainException` se comparan por `Error.Code`.
- [ ] Los objetos válidos salen del object mother.
- [ ] Los límites de los value objects usan `MaxLength` con el `NOTE` de la columna.
- [ ] Lo raro que apareció quedó con un `TODO:` en el test y está en el reporte, sin ajustar el test
      ni el código.
- [ ] El build da 0 warnings y `dotnet test` pasa entero.
