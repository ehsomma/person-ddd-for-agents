# Handoff: editorconfig-vs-teamStandars

## Objetivo

El usuario quiere que el código que escribe el agente cumpla StyleCop, `.editorconfig`, `Directory.Build.props`,
`Directory.Build.targets` y el skill `team-standards`, y sospechaba que había reglas mezcladas o contradictorias.
Preguntas que hizo:

- ¿Cuál es el proceso del agente para aplicar las reglas? ¿Primero el skill y después corrige los warnings del build?
- ¿Qué pasa con las reglas en `none`, `silent` y `suggestion`? Por ejemplo `csharp_prefer_braces = true:silent`.
- ¿Un CI con `TreatWarningsAsErrors` puede fallar aunque el build local del agente pase?
- ¿Qué reglas se contradicen entre el `.editorconfig` y el skill?

El objetivo final: que VS, el build local y el CI vean **las mismas reglas**, y que el skill no contradiga al
`.editorconfig`.

## Diagnóstico

- **`dotnet build` no ejecutaba ninguna regla IDE (`IDExxxx`).** El SDK deja `EnforceCodeStyleInBuild` en `false`
  por defecto (`C:\Program Files\dotnet\sdk\10.0.402\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.Analyzers.targets:99`)
  y, sin esa propiedad, no agrega los analyzers de code style al build (línea 151). El build veía CS, CA, SA y
  ARCH. Lo `csharp_style_*`, `dotnet_style_*`, naming (IDE1006) y formato (IDE0055) solo lo veía el Error List
  de VS.
- **VS y el build resuelven distinto el sufijo `= valor:silent`**. Se verificó con un archivo temporal:
  - En VS gana `dotnet_analyzer_diagnostic.category-Style.severity = warning`, así que IDE0008 e IDE0022 salen como
    warning.
  - En el build (SDK 9+) gana el sufijo: con `:silent` no salen. Sin sufijo salen como warning, igual que en VS.
  - La severidad de las naming rules (`dotnet_naming_rule.*.severity = suggestion`) la pisa la categoría en los
    dos lados: IDE1006 salía como warning.
- **`category-StyleCop` no hacía nada.** Las categorías de StyleCop son `StyleCop.CSharp.*Rules` (verificado en la
  DLL); las SA ya son warning por defecto. **`category-CodeQuality` es una categoría de reglas IDE** (IDE0051,
  IDE0052), no de CA. Las CA son warning por `AnalysisMode=All`, que agrega `analysislevel_10_all.globalconfig`.
- **Las reglas `silent` igual participan del Code Cleanup y de las Quick Actions.** Por eso el **valor** de la
  opción importa aunque la regla esté en `silent`. Si el valor prefiere lo contrario del skill, el Cleanup lo
  aplica.

### Contradicciones entre el `.editorconfig` y el skill

| # | Skill | `.editorconfig` |
|---|---|---|
| C1 | La excepción del "return" nombraba los "métodos de una sola expresión" como expression-bodied válidos. | `csharp_style_expression_bodied_methods = false`: IDE0022 es warning. |
| C2 | La línea en blanco iba solo antes de `return`. | `resharper_csharp_blank_lines_before_control_transfer_statements = 1` también la pone antes de `throw`, `break` y `continue`. El código estaba mezclado. |
| C3 | Justificaba "la línea en blanco va antes del comentario" con SA1512. | `SA1512 = silent`, así que no se enforza. |
| C4 | No decía nada de collection expressions (el usuario no las usa, ver el handoff `person-repository`). | `dotnet_style_prefer_collection_expression` no estaba definido, y el default prefiere `[]`. Había un `new[] { … }` en `UpdatePersonEndpoint`. |
| C5 | "El return devuelve solo una variable". | `dotnet_style_prefer_conditional_expression_over_return = true` prefiere `return c ? a : b;`. |
| C6 | Su premisa: "el .editorconfig cubre lo que los analyzers enforzan". | Las reglas IDE no corrían en el build. |

## Qué se hizo

Sin commitear al momento de escribir este handoff: se commitea junto con este archivo.

| Archivo | Cambio | Por qué |
|---|---|---|
| `Directory.Build.props` | `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>`. | El build y el CI ejecutan las mismas reglas IDE que VS. |
| `Directory.Build.props` | `<TreatWarningsAsErrors>` condicionado a `$(CI)`, `$(TF_BUILD)` o `$(ContinuousIntegrationBuild)`. | En el CI cualquier warning rompe el build; en local se permiten mientras se desarrolla. GitHub Actions y GitLab setean `CI`; Azure DevOps setea `TF_BUILD`. |
| `.editorconfig` | Se sacaron todos los sufijos `:silent`/`:suggestion` de las opciones de code style. | Con sufijo, la misma regla era warning en VS y silent en el build. Sin sufijo, en VS no cambia nada (ya eran warning) y el build coincide. |
| `.editorconfig` | Se sacó `category-StyleCop`. La cabecera ahora documenta de dónde sale la severidad de cada familia (SA, CA e IDE) y la nota sobre los sufijos. | I1: la línea no hacía nada y el comentario era incorrecto. |
| `.editorconfig` | Las naming rules pasan de `suggestion` a `warning`. | Es su severidad efectiva real. |
| `.editorconfig` | `dotnet_style_prefer_collection_expression = never`. | C4: el Code Cleanup ya no convierte a `[]`. |
| `.editorconfig` | `dotnet_style_prefer_conditional_expression_over_return = false`. | C5. |
| `.editorconfig` | Comentario de ReSharper actualizado: control transfer = estándar del skill. | C2. |
| `.editorconfig` | "Overrides por patrón de archivo" pasa a estar sobre las secciones `[*Xxx.cs]`, y las reglas globales de ahí quedaron bajo "Otros overrides (IDE y CA)". Se agregaron los títulos que faltaban (CA1848, CA2007, CA1822, CA1305, CA1863, SA1650, SA1208, CA1008, CA1873, CA1711, CA1812, CA1062). | I3 e I4: la convención de `CLAUDE.md` pide código y título en cada override. |
| `.claude/skills/team-standards/SKILL.md` | Intro: el build verifica SA, CA e IDE, y las preferencias sobre reglas `none`/`silent`/`suggestion` van en el skill. | C6. |
| `SKILL.md` | La excepción del "return" queda en propiedades, indexers, accessors y lambdas. | C1. |
| `SKILL.md` | La regla pasa a ser "Línea en blanco antes de `return`, `throw`, `break` y `continue`", con ejemplos de `throw;` y de `break` en un `case`. SA1512 en `silent`: se respeta a mano. | C2 y C3. |
| `SKILL.md` | "Tipo explícito en el new" incluye las colecciones: ni `[]` ni `new[]`. | C4. |
| `CLAUDE.md` | Comando `dotnet build Records.sln -p:CI=true`. Sección nueva "Cómo se aplican las reglas de código". La regla de que no se usan sufijos. Fila nueva en la tabla de handoffs. | |
| `CreatePersonCommandHandler.cs` | `[SuppressMessage("CodeQuality", "IDE0052:...")]` en `_settings`. | Fue el único warning nuevo al activar las reglas IDE. VS ya lo marcaba: el comentario de ReSharper no silencia a Roslyn. El campo se deja a propósito como ejemplo. |
| `UpdatePersonEndpoint.cs` | `new[] { idMismatchError }` → `new ValidationError[] { idMismatchError }`. | C4. |
| `GlobalExceptionHandler.cs`, `CommandHandler.cs`, `DbSession.cs`, `InterceptedDbCommand.cs`, `ThrowException1/2Endpoint.cs` | 16 líneas en blanco antes de `throw` y `break`. | C2. |

### Decisiones (tomadas con el usuario)

- **Activar `EnforceCodeStyleInBuild`**: VS = build = CI.
- **Línea en blanco antes de `return`, `throw`, `break` y `continue`**. Las alternativas eran "solo `return`" o
  "`return` y `throw`", sacando el seteo de ReSharper. Se eligió la opción completa: queda igual a lo que hace
  ReSharper y el Code Cleanup lo aplica solo.
- **Sacar los sufijos**, en vez de dejarlos como referencia con un comentario. La prueba mostró que no eran solo
  decorativos: hacían que el build difiriera de VS.
- **`TreatWarningsAsErrors` solo en CI**, condicionado en `Directory.Build.props`. Se descartaron "lo pone el
  pipeline" y "siempre".
- **SA1512 sigue en `silent`**: estaba así desde la estructura inicial. Solo se corrigió el texto del skill, que la
  citaba como si la enforzara.
- **IDE0052 se suprime en el miembro** con `[SuppressMessage]`, como en el resto del repo, en vez de bajar la regla
  globalmente.

## Verificación hecha

- `dotnet build Records.sln --no-incremental -p:CI=true`: 0 warnings, 0 errores.
- `dotnet build Records.sln`: 0 warnings, 0 errores.
- `dotnet msbuild ... -getProperty:TreatWarningsAsErrors`: `false` en local, y `true` con `CI=true`, con
  `TF_BUILD=True` y con `-p:ContinuousIntegrationBuild=true`.
- Archivo temporal con `var x = 1;`: warning IDE0008 en local y error en modo CI. Se borró.
- Error List de VS (con `mcp__ide__getDiagnostics`): sin warnings. Quedan solo `suggestion` intencionales (CA1305,
  CA1863, IDE0001, IDE0270 e IDE0045).

## Pendiente

- **Ninguna tarea de esta sesión quedó abierta.**
- Inconsistencia menor que no se tocó: el ejemplo de `CLAUDE.md`
  (`dotnet_diagnostic.IDE0290.severity = silent # IDE0290: Use primary constructor`) usa un comentario inline, pero
  el `.editorconfig` pone siempre el comentario en la línea de arriba.
- Finales de línea de las copias de trabajo mezclados en el repo (78 `.cs` en LF, 73 en CRLF y 1 mixto). Ya estaba
  así antes de esta tarea. El índice está todo en LF (autocrlf), así que no afecta los commits.

## Próximos pasos concretos

1. **Al crear el pipeline de CI:** alcanza con `dotnet build Records.sln`. GitHub Actions y Azure DevOps setean
   `CI`/`TF_BUILD` solos. En un CI que no los setea (por ejemplo Jenkins) hay que pasar
   `-p:ContinuousIntegrationBuild=true`.
2. **Para reproducir el CI en local:** `dotnet build Records.sln -p:CI=true`.
3. **Para aflojar una regla:** `dotnet_diagnostic.<ID>.severity` con un comentario arriba (código y título).
   Nunca un sufijo en la opción.
4. **Si el equipo quiere una forma concreta sobre una regla en `none`/`silent`/`suggestion`:** escribirla en el skill
   y poner el **valor** de la opción a favor de esa forma, porque el Code Cleanup aplica el valor aunque la regla
   esté en `silent`.
5. **Al migrar a .NET 11:** desde ese nivel, `Microsoft.CodeAnalysis.CSharp.CodeStyle.targets` (líneas 50-53)
   agrega siempre `analysislevelstyle_all.globalconfig`, que pone cada regla IDE en warning por ID. Los
   `dotnet_diagnostic.<ID>` del `.editorconfig` siguen ganando, así que el resultado debería ser el mismo, pero hay
   que volver a correr el build y comparar con VS.

## Gotchas

- **El sufijo `:severity` se resuelve distinto en VS y en el build** (ver Diagnóstico). Antes de verificarlo, el
  agente había afirmado que la categoría siempre lo pisaba, y eso era cierto solo en VS.
- **`dotnet_analyzer_diagnostic.category-<X>` tiene que coincidir exacto con la categoría del diagnóstico.**
  StyleCop usa `StyleCop.CSharp.DocumentationRules`, etc. Las CA usan `Design`, `Naming`, `Performance`, etc.
- **Con `AnalysisMode` las entradas bulk (`category-*`) no afectan a las CA**, porque el globalconfig setea cada CA
  por ID. A las IDE sí las afectan en .NET 10: el globalconfig de estilo solo se agrega si `AnalysisModeStyle` es
  distinto de `AnalysisMode`, o si el nivel es 11 o más.
- **`silent` y `none` no impiden que VS genere o convierta código en ese estilo** (Code Cleanup, Quick Actions,
  generación de código). Lo que manda es el valor de la opción.
- **El comentario `// ReSharper disable once ...` no silencia a Roslyn.** Si VS muestra el warning, hace falta un
  `[SuppressMessage]` o un `#pragma`.
- **`mcp__ide__getDiagnostics`:** severity 2 = warning y 3 = suggestion. Las entradas con severity 1 del tipo
  "archivo(n): +x -y" son marcadores de cambios del editor (contra la línea base de VS, no de git), no errores.
- **Para probar la precedencia de una opción sin tocar el `.editorconfig` raíz** sirve un `.editorconfig` anidado
  temporal, con una sección `[Archivo.cs]` que redefina la opción. Se aplica encima del raíz.
