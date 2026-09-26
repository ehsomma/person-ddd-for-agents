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
