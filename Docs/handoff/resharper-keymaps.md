# Handoff: resharper-keymaps

## Objetivo

Recuperar tres comportamientos de ReSharper en Visual Studio que el usuario había perdido:

1. `Alt+Enter` sobre código seleccionado debe abrir el menú contextual de ReSharper (había pasado a
   abrir otra cosa).
2. El quick-fix que agrega el tag `<param>` faltante al comentario XML de un método o constructor.
3. `Ctrl+T` debe abrir el buscador de ReSharper (Go to Everything), que incluye archivos.

Es una tarea de configuración del IDE: **no se tocó ningún archivo del repo** (salvo este handoff).

## Qué se hizo

### 1. `Alt+Enter` → menú de ReSharper ✅ resuelto

Otro comando se había quedado con el atajo (lo más probable: `View.QuickActions` de VS, que también usa
`Alt+Enter`). Se recuperó con alguna de estas dos opciones:

- **Reaplicar el esquema:** `ReSharper → Options → Environment → Keyboard & Menus → Apply Scheme`.
  Reasigna todos los atajos de ReSharper, pero pisa los atajos personalizados de VS.
- **Asignarlo a mano:** `Tools → Options → Environment → Keyboard`, comando
  `ReSharper.ReSharper_AltEnter`, scope **Text Editor**, `Alt+Enter` → **Assign**. Opcional: sacarle
  `Alt+Enter` a `View.QuickActions` para que no haya conflicto.

### 2. Quick-fix para agregar `<param>` ✅ resuelto (no estaba roto)

Causa: el constructor de `CreatePersonCommandHandler` tenía solo `<summary>`, **sin ningún `<param>`
documentado**. En ese caso:

- El compilador **no** emite `CS1573`, porque solo lo emite cuando otros parámetros sí tienen
  `<param>`. El quick-fix de ReSharper para agregar el `<param>` está asociado a ese warning.
- El warning que sí aparece es **`SA1611`** (StyleCop). ReSharper lo muestra como
  "Roslyn analyzer: SA1611…", pero sin su fix para agregar el `<param>`.

El usuario confirmó que, con un `<param>` ya documentado, el fix aparece para los parámetros que faltan.

Se revisó la config del repo y no hay nada que interfiera: `.editorconfig` no toca `CS1573` ni
`SA1611`, y `Directory.Build.props` tiene `GenerateDocumentationFile=true`.

### 3. `Ctrl+T` → Go to Everything de ReSharper ✅ resuelto

Mismo problema que el punto 1: otro comando se quedó con el atajo. El sospechoso es `Edit.GoToAll`, el
buscador propio de VS 2022, que también usa `Ctrl+T` y suele recuperarlo después de una actualización
de VS. **Se resolvió asignándolo a mano** (el usuario confirmó que funcionó):

- `Tools → Options → Environment → Keyboard`, comando `ReSharper.ReSharper_GotoType` (es Go to
  Everything, aunque el nombre diga "Type"), scope **Global** o **Text Editor**, `Ctrl+T` →
  **Assign**. Opcional: sacarle `Ctrl+T` a `Edit.GoToAll`.

Se eligió esta opción en lugar de reaplicar el esquema porque no pisa los atajos personalizados de VS.

## Pendiente

Nada.

## Próximos pasos

Ninguno obligatorio. Si vuelve a pasar:

- Un atajo de ReSharper roto (típico después de actualizar VS o ReSharper): en
  `Tools → Options → Environment → Keyboard`, apretar el atajo en **Press shortcut keys** y mirar
  **Shortcut currently used by** para ver quién lo tiene. Después, reaplicar el esquema o reasignarlo a
  mano como en los puntos 1 y 3.
- Método sin ningún `<param>`: borrar el bloque `///` y volver a escribir `///` para que VS genere todos
  los `<param>`, o documentar uno a mano y usar el quick-fix de ReSharper para el resto.

## Gotchas

- Las actualizaciones de VS le devuelven los atajos a sus comandos nativos (`View.QuickActions` para
  `Alt+Enter`, `Edit.GoToAll` para `Ctrl+T`). Reaplicar el esquema de ReSharper los recupera todos
  juntos, pero pisa los atajos personalizados de VS.
- Si lo que se busca es solo archivos, el comando de ReSharper es **Go to File**
  (`ReSharper.ReSharper_GotoFile`, por defecto `Ctrl+Shift+T`).
- `SA1611` ≠ `CS1573`: el quick-fix de ReSharper para agregar el `<param>` depende de `CS1573`, que solo
  existe si ya hay al menos un `<param>`.
- Si el quick-fix no aparece ni siquiera con un `<param>` documentado, revisar
  `ReSharper → Options → Code Inspection → Inspection Severity → "no matching param tag"` (que no esté
  en *Do not show*) y que **Enable code analysis** esté activo en `Code Inspection → Settings`.
- El MCP `rider` falló al conectar en esta sesión (ECONNREFUSED). No afectó la tarea porque el IDE es
  Visual Studio + ReSharper.
