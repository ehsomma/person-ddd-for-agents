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
