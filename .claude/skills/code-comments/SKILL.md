---
name: code-comments
description: Usar SIEMPRE al escribir, modificar o revisar comentarios en código C# de este repositorio (XML docs `///`, comentarios inline `//`, `NOTE:`/`TODO:` y el `Justification` de los `SuppressMessage`), o sea, casi siempre que se escribe C#. Define en qué idioma va cada comentario según la carpeta (XML docs en inglés en Core/BuildingBlocks y en español es-AR en Modules/Apps; inline siempre en español), el tono según quién lo lee (API, técnico, negocio), qué tiene que explicar un comentario, el formato de los XML docs (en bloque o en una línea, corte en la columna 100) y los tags permitidos. Complementa a `team-standards`. Si un archivo existente (aunque sea de referencia) contradice esta skill, gana la skill.
---

# Comentarios de código

Ningún analyzer revisa el idioma, el tono ni el formato de los comentarios: StyleCop solo exige que
los XML docs existan y algunos detalles (ver [Lo que ya valida el build](#lo-que-ya-valida-el-build)).
Así que estas reglas se aplican a mano.

**Al dueño del repo le gustan los comentarios.** El agente escribe el código y una persona lo lee a
través de sus comentarios: ante la duda, se comenta. No molesta que haya más comentario que código.

**Lo que ya está, ya está.** Estas reglas son para los comentarios nuevos y para los que se
reescriben. **Nunca se traduce ni se reformatea un comentario existente** salvo que el usuario lo pida
explícitamente (ver [Editar un comentario existente](#editar-un-comentario-existente)). Del código
existente, incluidos los archivos de referencia del `CLAUDE.md`, se imitan la estructura y el código,
no los comentarios viejos: si contradicen esta skill, gana la skill.

Es un documento vivo: las reglas nuevas se agregan con el mismo formato que `team-standards`
(regla, excepción, por qué, ✗/✓).

## Idioma según la carpeta

El idioma depende de la carpeta del archivo y del tipo de comentario:

| Tipo de comentario | `Src/Core`, `Src/BuildingBlocks` | `Src/Modules`, `Src/Apps` |
|---|---|---|
| XML docs (`///`) | Inglés | Español |
| Inline (`//`, incluidos `NOTE:` y `TODO:`) | Español | Español |
| `Justification` de los `SuppressMessage` | Español | Español |

Van **siempre en inglés**, en cualquier carpeta:

- Los identificadores (clases, interfaces, métodos, propiedades, variables, etc.).
- Los mensajes y códigos de error (`ERR.PERSON.NOTFOUND`, `"Person not found."`) y los mensajes de log.
- Los tags `NOTE:` y `TODO:` (el texto que sigue va en español).
- El marcador `// Using aliases.` de los using aliases (es un texto fijo, ver `team-standards`).

**Por qué:** Core y BuildingBlocks son técnicos y vienen heredados de otros proyectos, así que sus XML
docs siguen en inglés. Modules y Apps son el negocio y la API, y se leen en español. Los comentarios
inline y las justificaciones son notas para las personas del equipo, así que van en español en todo el
repo. La regla es por carpeta (y no por capa) para cubrir también los proyectos que no son una capa
del negocio: `Configuration`, `Dtos`, `Infra.Configuration.DependencyInjection` y `Program.cs`.

✗ Incorrecto (en `Src/BuildingBlocks`):
```csharp
/// <summary>
/// Publica los domain events en orden.
/// </summary>
public async Task Publish(IEnumerable<IDomainEvent> domainEvents)
{
    // Sequential (not Task.WhenAll): the handlers share Scoped dependencies.
```

✓ Correcto (en `Src/BuildingBlocks`):
```csharp
/// <summary>
/// Publishes the domain events in order.
/// </summary>
public async Task Publish(IEnumerable<IDomainEvent> domainEvents)
{
    // Secuencial (no Task.WhenAll): los handlers comparten dependencias Scoped (la conexión y el UoW
    // de Dapper) que no son thread-safe.
```

## Español es-AR y spanglish

Cuando el comentario va en español:

1. **Es español de Argentina, con tildes.** "recién", "después", "público", "común", "así".
2. **Los términos técnicos se dejan en inglés** si se entienden mejor así: aggregate, aggregate root,
   value object, domain event, handler, command, query, request, endpoint, mapper, validator, commit,
   rollback, round trip, splitOn, etc. No se fuerzan traducciones como "manejador", "agregado" o
   "confirmar la transacción".
3. **Los identificadores se escriben tal cual, en inglés.** En los XML docs se referencian con
   `<see cref="..."/>` (tipos y miembros) o `<paramref name="..."/>` (parámetros). En los inline se
   escribe el nombre (`Person.Delete`, `DomainErrors.Person.NotFound`).
4. **Los conceptos de negocio van en español y en minúscula**, con el `<see cref>` al tipo la primera
   vez que aparecen en el comentario: "una nueva persona (`<see cref="Person"/>`)", "los bienes
   personales (`<see cref="PersonalAsset"/>`)". La mayúscula es solo para el identificador.
5. **Registro:**
   - Los XML docs se escriben en tercera persona: "Representa…", "Devuelve…", "Crea…". La
     excepción es el `<summary>` de los endpoints, que va en infinitivo (ver
     [Tono según quién lo lee](#tono-según-quién-lo-lee)).
   - El voseo se usa solo cuando el comentario le habla al lector, por ejemplo en un `NOTE` o un
     `TODO` que indica qué hacer: "si agregás un campo, actualizá también el mapper".
   - Nunca se usa "tú" ni "usted".

**Por qué:** en Argentina los términos técnicos se usan en inglés, y traducirlos los hace más difíciles
de reconocer. El `<see cref>` deja claro a qué tipo se refiere el concepto, IntelliSense lo convierte
en un link y el compilador avisa si el tipo cambia de nombre (CS1574).

✗ Incorrecto (en `Src/Modules`):
```csharp
/// <summary>
/// Represents a command handler for creating a new person.
/// </summary>

/// <summary>
/// Representa el manejador del comando para crear una nueva Persona.
/// </summary>
```

✓ Correcto (en `Src/Modules`):
```csharp
/// <summary>
/// Representa el command handler para crear una nueva persona (<see cref="Person"/>).
/// </summary>
```

## Editar un comentario existente

- **Si el cambio es parcial** (corregir un dato, agregar una frase), se respetan el idioma y el
  formato que ya tiene el comentario.
- **Si se reescribe entero** (porque el código cambió y el comentario ya no sirve), va en el idioma y
  el formato de esta skill.
- **Si el cambio de código deja un comentario desactualizado, se actualiza en el mismo cambio.** Eso no
  es traducir: un comentario que dice algo falso es peor que no tener comentario.
- Nunca se traducen ni se reformatean de pasada los comentarios que el cambio no toca.

**Por qué:** traducir o reformatear comentarios ajenos al cambio llena el diff de ruido y esconde el
cambio real. Pero un comentario desactualizado engaña al que lo lee, así que ese sí se corrige.

## Tono según quién lo lee

### API: lo que se ve en Scalar

Son los XML docs de los métodos de endpoint y de los DTOs (`*.Dtos`). Los leen terceros, así que
el tono es **formal y claro, sin jerga interna**: nada de aggregate, handler, `GlobalExceptionHandler`
o Dapper. Se describe qué hace o qué representa desde el punto de vista del que consume la API.

1. **El `<summary>` del endpoint es un título corto en infinitivo** (`Crear persona.`,
   `Obtener persona por ID.`), porque es lo que Scalar muestra en el menú.
2. **El `<remarks>` explica solo qué hace el endpoint.** Las validaciones, los códigos de error y los
   casos borde van en las `<response code="...">`.
3. **En los DTOs, el `<summary>` dice qué es el dato y el `<example>` lleva un valor realista**, sin
   punto final (SA1629 está suprimida en los DTOs para eso).

El resto del código de `Src/Apps` (validators, `Program.cs`, los comentarios inline de los endpoints)
lo lee el equipo, no los terceros, así que sigue el tono de negocio o técnico.

**Por qué:** el `<summary>` es el título de la operación en el menú de Scalar y tiene que distinguir de
un vistazo los endpoints que comparten ruta (`GET`, `PUT` y `DELETE /persons/{id}`). La ruta ya se ve
en el detalle. En el `<remarks>` no se repiten las validaciones ni los errores porque, si cambian, es
fácil olvidarse de actualizar ese texto.

✗ Incorrecto:
```csharp
/// <summary>
/// PUT persons/{id}.
/// </summary>
/// <remarks>
/// Carga el aggregate con el PersonService y lo actualiza. Si el ID no coincide devuelve un 400.
/// </remarks>
```

✓ Correcto:
```csharp
/// <summary>
/// Actualizar persona.
/// </summary>
/// <remarks>
/// Actualiza los datos de una persona existente, le agrega los personal assets nuevos que vengan en
/// el body y devuelve la persona tal como quedó guardada.
/// </remarks>
/// <response code="200">Persona actualizada.</response>
/// <response code="404">La persona no existe.</response>
```

### Core y BuildingBlocks

Técnico, pero escrito para que lo entienda una persona del equipo leyendo el código, no para generar
documentación. Lo que más importa es explicar las decisiones que no son obvias: por qué algo es
secuencial, por qué se usa `CancellationToken.None`, por qué un método tiene que ser público.

### Negocio: Domain, Application, Infra.Persistence.Sql e Infra.Queries.Sql

Comentarios **bien claros y abundantes** (ver la sección siguiente). También se documentan con XML
docs los métodos privados que tengan lógica, aunque SA1600 no lo exija.

## Comentar los bloques de complejidad media y alta

En el código de negocio, todo bloque de complejidad media o alta lleva arriba un comentario que
explica **qué hace y, sobre todo, por qué**. Cuentan como complejidad media o alta:

- Una decisión que tenía alternativas (por qué `||` y no `&&`, por qué secuencial y no `Task.WhenAll`).
- Un orden que importa (primero se borran los hijos por las FKs, los eventos se publican después del
  commit).
- Una regla de negocio o un caso borde (qué pasa si viene `null`, si no existe, si viene sin ID).
- Un efecto colateral que no se ve en la firma (registra un evento, abre una transacción).
- Una consulta o un LINQ que no se entiende de un vistazo (multi-mapping, `splitOn`, agrupaciones).
- Algo que parece un error pero es a propósito (un `SELECT` sin `NOLOCK`, un `!` sobre un nullable).

El comentario puede incluir:

- **Ejemplos con datos concretos** (entrada → resultado), alineados para que se lean como una tabla.
- **Analogías**, si el concepto es abstracto.
- **Emojis, solo si suman claridad:** ✓ y ✗ para los casos válidos e inválidos, ⚠️ para una trampa.
  Como máximo uno por línea y nunca decorativos.

Se prefiere explicar el porqué y el contexto antes que traducir el código a palabras: un comentario que
solo repite la línea de abajo no ayuda a entender nada que no se vea ya en el código.

**Por qué:** el código dice qué hace, pero no por qué se eligió esa forma ni qué se rompe si se cambia.
Ese contexto es lo que se pierde primero, y es lo que necesita el que lee el código para modificarlo
sin miedo.

✗ Incorrecto:
```csharp
// Si lat o lng no son null, crea el LatLng.
LatLng? latLng = null;
if (dataAddress.Lat != null || dataAddress.Lng != null)
{
    latLng = LatLng.Build(dataAddress.Lat, dataAddress.Lng);
}
```

✓ Correcto:
```csharp
// La dirección puede no tener coordenadas. Se usa "||" (y no "&&") a propósito: una fila con una
// sola de las dos (dato corrupto) llega igual a LatLng.Build, que lanza la excepción en vez de
// descartarla en silencio.
//   Lat = -34.60, Lng = -58.38  → LatLng válido ✓
//   Lat = null,   Lng = null    → sin coordenadas (latLng queda en null)
//   Lat = -34.60, Lng = null    → LatLng.Build lanza la excepción ⚠️
LatLng? latLng = null;
if (dataAddress.Lat != null || dataAddress.Lng != null)
{
    latLng = LatLng.Build(dataAddress.Lat, dataAddress.Lng);
}
```

## Formato de los XML docs

1. **En bloque:** el `<summary>`, el `<remarks>` y el `<example>` de los tipos (clases, records,
   structs, interfaces y enums) y de los métodos (incluidos los factory methods `Create`, `Load` y
   `Build`). El tag de apertura y el de cierre van cada uno en su línea, y el texto en el medio.
2. **En una sola línea:** el `<summary>` de los constructores, los fields, las constantes, las
   propiedades y los items de enum, aunque pase la columna 100.
3. **En su misma línea, aunque pase la columna 100:** todos los demás tags (`<param>`, `<typeparam>`,
   `<returns>`, `<exception>`, `<response>`, `<value>` y el `<example>` de las propiedades).
4. **Párrafos:** dentro de un `<summary>` o un `<remarks>` se separan con `<para>`, no con una línea
   `///` vacía, porque IntelliSense las junta en un solo párrafo.
5. **Implementaciones:** los overrides y las implementaciones de interfaz llevan `<inheritdoc />`, y la
   documentación vive en la interfaz o en la clase base. Si la implementación agrega algo propio (por
   ejemplo una excepción), va debajo del `<inheritdoc />`.
6. **Textos fijos:**
   - El `<summary>` del constructor es `Inicializa una nueva instancia de <see cref="X"/>.` (en Core y
     BuildingBlocks, `Initializes a new instance of the <see cref="X"/> class.`).
   - El `ArgumentNullException` del constructor es
     `Cuando alguno de los parámetros es <see langword="null"/>.`
   - Las palabras clave van con `<see langword="..."/>` (`null`, `true`, `false`), y los fragmentos de
     código con `<c>...</c>`.

**Por qué:** en bloque, una descripción larga se lee mejor y el diff muestra solo la línea que cambió.
En una línea, las listas de propiedades quedan compactas (dos líneas por propiedad). Ninguna regla de
StyleCop ni de Roslyn controla este formato, y el formateo de XML docs de ReSharper solo actúa al
reformatear, no en el build.

✗ Incorrecto:
```csharp
/// <summary>Representa el command handler para eliminar una persona existente.</summary>
internal sealed class DeletePersonCommandHandler : CommandHandler<DeletePersonCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePersonCommandHandler"/> class.
    /// </summary>
    /// <param name="personService">
    /// El servicio de dominio de <see cref="DomainModel.Person"/>.
    /// </param>
```

✓ Correcto:
```csharp
/// <summary>
/// Representa el command handler para eliminar una persona existente
/// (<see cref="DomainModel.Person"/>).
/// </summary>
internal sealed class DeletePersonCommandHandler : CommandHandler<DeletePersonCommand>
{
    /// <summary>Inicializa una nueva instancia de <see cref="DeletePersonCommandHandler"/>.</summary>
    /// <param name="personService">El servicio de dominio de <see cref="DomainModel.Person"/>.</param>
```

## Largo de línea: columna 100

El texto en bloque de los XML docs y los comentarios inline se cortan en la **columna 100**, contada
desde el principio de la línea (con la indentación incluida). No es estricto: si una palabra queda
pasada, no pasa nada. Las URLs y los fragmentos de código no se parten.

**Excepción:** las líneas que según [Formato de los XML docs](#formato-de-los-xml-docs) van en una sola
línea (`<param>`, `<returns>`, el `<summary>` de las propiedades, etc.).

**Por qué:** a 100 columnas el comentario se lee sin scroll horizontal en VS con el Solution Explorer
abierto y en un diff lado a lado. Ningún analyzer lo controla: StyleCop no tiene una regla de largo de
línea, y el `max_line_length` del `.editorconfig` está comentado.

## Tags `NOTE:` y `TODO:`

Los únicos tags permitidos son `NOTE:` y `TODO:`, en mayúsculas, en inglés y con dos puntos. No se
usan `NOTA:`, `INFO:`, `HACK:`, `FIXME:` ni otros.

- **`NOTE:`** marca algo no obvio que, si no se sabe, lleva a romper algo o a "arreglar" algo que
  está así a propósito.
- **`TODO:`** marca trabajo pendiente y dice qué falta y dónde debería ir (o por qué no se hizo
  todavía). Nunca un `TODO: arreglar` suelto.

**Por qué:** con pocos tags fijos se encuentran todos con una búsqueda, y cada uno significa siempre lo
mismo.

✗ Incorrecto:
```csharp
// NOTA: ojo con esto.
// TODO: revisar.
```

✓ Correcto:
```csharp
// NOTE: Secuencial (no Task.WhenAll): los inserts comparten la misma conexión y transacción, y
// SqlConnection no soporta comandos concurrentes (ni siquiera con MultipleActiveResultSets).

// TODO: Las entidades hijas nuevas tienen ID 0 hasta que se guardan, así que dos entidades nuevas
// distintas son "iguales". Hay que decidir si una entidad sin ID solo es igual a sí misma.
```

## Código comentado

El código comentado se marca con `////` (SA1005 exige un espacio después de `//`, salvo en `////`) y
lleva el motivo por el que se deja. Si no hay un motivo, se borra: el historial ya queda en git.

✓ Correcto:
```csharp
////app.UseSerilogRequestLogging(); // Se activa cuando se quiera loguear cada request.
```

Para un bloque de varias líneas también vale `/* ... */`:

✓ Correcto:
```csharp
/*
Se activa cuando se quiera loguear cada request.
app.UseSerilogRequestLogging();
app.UseHttpLogging();
*/
```

## `Justification` de los `SuppressMessage`

Va en español y explica por qué la regla no aplica en ese caso. No repite el nombre de la regla.

✗ Incorrecto:
```csharp
Justification = "Checked."
```

✓ Correcto:
```csharp
Justification = "Create() y Load() van dentro de la region Constructor."
```

## Lo que ya valida el build

Estas reglas las marca el build como warning, así que no hace falta repetirlas en cada revisión,
pero hay que respetarlas:

- **SA1600:** los elementos públicos e internos tienen que estar documentados.
- **SA1611 y SA1615:** un `<param>` por parámetro y un `<returns>` si el método devuelve algo.
- **SA1612:** los `<param>` van en el mismo orden que los parámetros.
- **SA1629:** el texto de la documentación termina en punto.
- **SA1005:** los comentarios `//` llevan un espacio después de las barras.
- **CS1574:** un `<see cref>` que no se resuelve.

Están apagadas a propósito en el `.editorconfig`: SA1623 y SA1642 (el texto estándar de las
propiedades y los constructores; acá se usa el de [Formato de los XML docs](#formato-de-los-xml-docs))
y SA1650 (la ortografía, que no entiende el español).

## Checklist antes de cerrar

- [ ] Cada comentario está en el idioma que le corresponde según la carpeta y el tipo (los inline y
      las justificaciones, siempre en español).
- [ ] El español tiene tildes, los términos técnicos quedaron en inglés y los identificadores se
      escriben tal cual o con `<see cref>`.
- [ ] Los XML docs de los tipos y los métodos van en bloque, y los de los constructores, las
      propiedades, los fields y los items de enum, en una línea.
- [ ] El texto en bloque y los inline cortan en la columna 100.
- [ ] Los bloques de complejidad media o alta del negocio explican el porqué.
- [ ] Solo se usaron `NOTE:` y `TODO:`.
- [ ] Los comentarios que el cambio dejó desactualizados se corrigieron.
- [ ] No se tradujo ni se reformateó ningún comentario que el cambio no tocaba.
