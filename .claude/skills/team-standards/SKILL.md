---
name: team-standards
description: Usar SIEMPRE al escribir, revisar o refactorizar código C# en este repositorio. Contiene estándares de código propios del equipo que no están cubiertos por .editorconfig/StyleCop/Roslyn (no existen como analyzer) y que hay que aplicar manualmente. Se va ampliando con nuevos estándares a medida que el equipo los define — antes de cerrar cualquier tarea de código, chequear que el cambio cumple todas las reglas listadas acá.
---

# Estándares de Código del Equipo

El `.editorconfig` de este repositorio cubre todo lo que StyleCop/Roslyn/IDE pueden enforcar automáticamente con un analyzer. Pero hay convenciones que el equipo quiere seguir y que no existen como regla de analyzer (ni en StyleCop.Analyzers ni en Roslynator), así que no se pueden poner ahí. Este skill es el lugar donde documentarlas para que se apliquen manualmente al escribir o revisar código.

Es un documento vivo: cada vez que el equipo defina un estándar nuevo que no se pueda expresar en `.editorconfig`, se agrega acá como una sección nueva al final del archivo, con el mismo formato que las anteriores:

````markdown
## <Título corto de la regla>

<Regla, explicada en 1-2 frases.>

**Excepción:** <si aplica; si no hay, omitir esta línea>

**Por qué:** <motivación de la regla>

✗ Incorrecto:
```csharp
<ejemplo que viola la regla>
```

✓ Correcto:
```csharp
<mismo ejemplo, cumpliendo la regla>
```
````

## El `return` debe devolver solo una variable, no una llamada inline

En métodos con cuerpo de bloque (`{ }`), el `return` debe devolver una variable ya asignada, no una llamada a función/método hecha directamente en la línea del return.

**Excepción:** no aplica a miembros expression-bodied (`=>`) — propiedades, accessors, lambdas, métodos de una sola expresión — porque ahí no hay bloque ni `return` explícito.

**Por qué:** nombrar la variable documenta qué representa el valor devuelto, en vez de dejar que el lector infiera el significado a partir de la expresión.

✗ Incorrecto:
```csharp
public Person GetPerson(int id)
{
    return _repository.FindById(id);
}
```

✓ Correcto:
```csharp
public Person GetPerson(int id)
{
    Person person = _repository.FindById(id);

    return person;
}
```

## Agrupar los miembros de las clases en regions

Dentro de una clase, los miembros se agrupan en `#region`s con estos nombres y en este orden: `Declarations` (fields y constantes), `Constructor` (constructores), `Properties` (propiedades), `Public methods` (métodos `public` e `internal`), `Protected methods` (métodos `protected`, `protected internal` y `private protected`) y `Private methods` (métodos privados). Los constructores van siempre en `Constructor`, sea cual sea su visibilidad. Las implementaciones explícitas de interfaz (p.ej. `Task IRequestHandlerBase.Handle(...)`) van en `Public methods`: aunque no llevan modificador de acceso, son API que otras clases consumen a través de la interfaz. Si la clase no tiene miembros de un tipo, esa region se omite (no se dejan regions vacías).

**Excepción:** no se usan regions en la sección de los `using` ni en las interfaces (sus miembros son todos públicos y casi siempre del mismo tipo, así que quedaría una sola region que no aporta nada).

**Por qué:** separar la clase en bloques con nombre hace que sea más fácil de leer y de navegar (se pueden colapsar en el IDE), y un orden fijo hace que siempre se sepa dónde buscar cada miembro. El orden de las regions respeta el que exige StyleCop (SA1201: constructores antes que propiedades), así el analyzer sigue validándolo. Los métodos se ordenan de más a menos visible (mismo criterio que SA1202); `internal` va con los públicos porque también es API que consumen otras clases. `SA1124` está desactivada en el `.editorconfig` justamente para permitirlo.

✗ Incorrecto:
```csharp
#region Usings

using System;

#endregion

public class PersonService
{
    private const int MaxNameLength = 100;
    private readonly IPersonRepository _repository;

    public PersonService(IPersonRepository repository)
    {
        _repository = repository;
    }

    public int Count { get; private set; }

    public Person GetPerson(int id)
    {
        Person person = _repository.FindById(id);
        return person;
    }

    private static bool IsValidName(string name)
    {
        bool isValid = name.Length <= MaxNameLength;
        return isValid;
    }
}
```

✓ Correcto:
```csharp
using System;

public class PersonService
{
    #region Declarations

    private const int MaxNameLength = 100;
    private readonly IPersonRepository _repository;

    #endregion

    #region Constructor

    public PersonService(IPersonRepository repository)
    {
        _repository = repository;
    }

    #endregion

    #region Properties

    public int Count { get; private set; }

    #endregion

    #region Public methods

    public Person GetPerson(int id)
    {
        Person person = _repository.FindById(id);
        return person;
    }

    #endregion

    #region Private methods

    private static bool IsValidName(string name)
    {
        bool isValid = name.Length <= MaxNameLength;
        return isValid;
    }

    #endregion
}
```

## Nombres cuando conviven la misma entidad de distintas capas

Cuando en un mismo archivo o método interactúan dos tipos con el mismo nombre de distintas capas (p.ej. el `Person` del dominio y el `Person` del modelo de datos o un DTO), se aplican dos reglas:

1. **Tipos:** se referencian con using aliases que nombran la capa (`DomainModel`, `DataModel`, `Dto`, etc.), y cada using alias termina con el comentario `// Using aliases.`.
2. **Variables y parámetros:** el del dominio va **sin prefijo** (`person`) y el de la otra capa lleva el prefijo de su capa (`dataPerson`, `personDto`).

**Excepción:** en los mappers, que traducen entre las dos representaciones y ninguna es "la principal", las dos llevan prefijo (`domainPerson` y `dataPerson`), igual que los parámetros `domainModel` / `dataModel` de `IPersistanceMapper`.

**Por qué:** el alias con el nombre de la capa hace explícito en cada uso de qué capa es el tipo, sin escribir el namespace completo y sin ambigüedades entre tipos homónimos. Para las variables, el dominio es el idioma por defecto del proyecto: los contratos (p.ej. `IPersonRepository`) se definen en el dominio sin prefijo, así que la implementación usa el mismo nombre de parámetro (lo exige CA1725) y el prefijo distingue a la representación que no es del dominio.

✗ Incorrecto:
```csharp
using Records.Persons.Infra.Persistence.Sql.PersonAggregate.Models;

public async Task InsertAsync(Records.Persons.Domain.PersonAggregate.Models.Person domainPerson)
{
    Person person = _personMapper.FromDomainToDataModel(domainPerson);
    await _dbSession.Connection.InsertAsync(person, _dbSession.Transaction);
}
```

✓ Correcto:
```csharp
using DataModel = Records.Persons.Infra.Persistence.Sql.PersonAggregate.Models; // Using aliases.
using DomainModel = Records.Persons.Domain.PersonAggregate.Models; // Using aliases.

public async Task InsertAsync(DomainModel.Person person)
{
    DataModel.Person dataPerson = _personMapper.FromDomainToDataModel(person);
    await _dbSession.Connection.InsertAsync(dataPerson, _dbSession.Transaction);
}
```

## Tipo explícito en el `new`

Al crear un objeto se escribe siempre el tipo en el `new` (`new Person(...)`), no el `new(...)` target-typed, aunque el tipo ya esté a la izquierda de la asignación.

**Por qué:** es el mismo criterio que con `var` (se escribe el tipo explícito): se lee más fácil y no hay que mirar el otro lado de la asignación (o la firma del parámetro) para saber qué se está creando. En el `.editorconfig` está `csharp_style_implicit_object_creation_when_type_is_apparent = false` e `IDE0090` apagada, pero no existe una regla que marque el `new(...)`, así que hay que respetarlo a mano.

✗ Incorrecto:
```csharp
GetPersonByIdQuery query = new(id);
private readonly List<IDomainEvent> _domainEvents = new();
InterceptedDbConnection connection = new(new SqlConnection(_connectionString));
```

✓ Correcto:
```csharp
GetPersonByIdQuery query = new GetPersonByIdQuery(id);
private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
InterceptedDbConnection connection = new InterceptedDbConnection(new SqlConnection(_connectionString));
```

## `WITH (NOLOCK)` en todos los `SELECT`

Toda tabla leída en un `SELECT` (incluidas las de los `JOIN`) lleva `WITH (NOLOCK)`, salvo que se especifique lo contrario para ese caso.

**Excepción:** no se usa en los `SELECT` que cargan un aggregate para un command (p.ej. `PersonRepository.GetByIdAsync`, que usan Update, Delete o AddPersonalAsset). Si leyera sin bloqueo un cambio todavía no confirmado de otra transacción que después hace rollback, el command decidiría y persistiría sobre datos que nunca existieron, y ese error queda guardado (en una consulta, en cambio, solo se mostraría un dato desactualizado por un momento).

**Por qué:** las lecturas no se bloquean contra las transacciones de escritura que estén abiertas (ni las bloquean). Se acepta a cambio la posibilidad de leer datos todavía no confirmados (dirty reads), que para las consultas del sistema no es un problema.

✗ Incorrecto:
```sql
SELECT PE.[Id], AD.[City]
FROM [dbo].[Persons] PE
    LEFT JOIN [dbo].[Addresses] AD ON AD.[PersonId] = PE.[Id]
WHERE PE.[Id] = @id;
```

✓ Correcto:
```sql
SELECT PE.[Id], AD.[City]
FROM [dbo].[Persons] PE WITH (NOLOCK)
    LEFT JOIN [dbo].[Addresses] AD WITH (NOLOCK) ON AD.[PersonId] = PE.[Id]
WHERE PE.[Id] = @id;
```
