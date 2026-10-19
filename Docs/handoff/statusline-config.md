# Handoff: statusline-config

## Objetivo

Configurar la status line de Claude Code, igual en las dos PCs del usuario (casa y oficina). Es una tarea de
configuración del entorno: **no se tocó ningún archivo del repo**, salvo este handoff.

Formato final:

```
Opus 5.5 (medium) | █░░░░░░░░░ 6% | 5h 1% (02:20) · 7d 6% (sáb 05:00)
person-ddd-for-agents (feature/mediator)
```

Primera línea:

1. **Modelo (effort):** en naranja Claude (`#D97757`, color de 24 bits).
2. **Contexto:** barra de 10 casilleros y el % de uso, los dos del mismo color (verde, amarillo o rojo
   según el uso).
3. **Límites del plan:**
   - **`5h`:** % usado de la ventana de 5 horas y la hora local en que se renueva.
   - **`7d`:** % usado del cupo semanal y el día y la hora local en que se renueva.

Segunda línea:

4. **Carpeta (rama):** solo el nombre de la carpeta actual en azul, y la rama de git en amarillo.

## Qué se hizo

### Archivos tocados (fuera del repo)

Los mismos dos archivos en cada PC. Cambia solo el usuario de Windows:

| PC | Carpeta |
|---|---|
| Casa | `C:\Users\Esteban\.claude\` |
| Oficina | `C:\Users\ehsomma\.claude\` |

- **`settings.json`:** `"statusLine": {"type": "command", "command": "bash ~/.claude/statusline-command.sh"}`.
  El resto de las keys quedó igual. En la oficina hay además un hook `Stop` con `notify.ps1`, que no se tocó.
- **`statusline-command.sh`:** el script que lee el JSON de stdin e imprime las dos líneas.

### Replicación en la PC de la oficina

- **Se borró la config vieja:** había un `statusline.py`, con su `statusline.py.bak`, que se llamaba con
  `python`. Mostraba `usuario@ → …/ruta git(rama ×)` y los límites como % restante. Se borraron los dos
  archivos y se cambió el `command` del `settings.json`.
- **El script se reescribió a partir de este handoff**, porque el original de casa no estaba disponible.
  Puede tener diferencias menores de implementación con el de casa, pero el formato que se ve es el mismo.
- **Se agregó un fix:** se llama a `git -C "$cwd/"` con barra final. Sin la barra, en la raíz de un disco
  (`N:`) git usa el directorio actual de ese disco y no la raíz. **Al script de casa le falta este fix.**

### Decisiones

- **Sin `jq`:** no está instalado (se verificó en la oficina). El script parsea el JSON solo con
  bash/sed/grep. La función `json_get <sección> <key>` devuelve el primer valor de `key` que aparece
  después de `"<sección>"`.
- **Formato base en vez de PS1:** el pedido original era convertir el PS1, pero no hay `.bashrc`,
  `.bash_profile`, `.zshrc` ni `.profile`. El usuario usa PowerShell, así que se armó un formato base y se
  fue ajustando.
- **Ruta corta:** se muestra solo la última carpeta (`${cwd##*/}`). En la raíz de un disco muestra `D:`.
- **Sin user ni host:** el usuario pidió sacar los dos.
- **Orden y colores:** el usuario pidió primero el modelo, después el contexto y al final la carpeta. El
  modelo y el effort van en el naranja del logo de Claude. El script arma las partes en un array y las
  une con ` | `, así que si falta alguna no quedan separadores sobrantes.
- **Effort:** sale de `effort.level` del JSON y se muestra entre paréntesis después del modelo. Antes de
  implementarlo se confirmó con un volcado real que ese campo existe. Si no viene, se muestra solo el
  modelo.
- **Barra de contexto:** sale de `context_window.used_percentage`. Se toma el primer valor después de
  `"context_window"` para no agarrar el de `rate_limits` (ese objeto tiene un `current_usage` anidado).
  - Tiene 10 casilleros, cada uno equivale al 10% y se redondea (`(pct+5)/10`).
  - La barra y el % van del mismo color: verde por debajo del 50%, amarillo por debajo del 80% y rojo
    desde el 80%. Antes el % salía en gris y el usuario pidió unificarlo.
- **Límites del plan (plan Team):** salen de `rate_limits.five_hour` y `rate_limits.seven_day`, cada uno
  con `used_percentage` y `resets_at`.
  - `resets_at` es un timestamp Unix y se pasa a hora local con `date -d @<ts>`. Se verificó que la zona
    horaria de Git Bash coincide con la de Windows (UTC-3) en las dos PCs.
  - Los % usan los mismos colores que el contexto. Si falta algún límite, no se muestra.
  - Los nombres de los días (`dom`…`sáb`) están en un array del script para no depender de la
    configuración de idioma de Git Bash.
  - Se eligió la opción B (hora y día exactos del reset). Se descartaron dos alternativas:
    - **A, tiempo restante con `↻4h12m`:** al usuario el ícono se le superponía con el dato.
    - **C, con mini barras:** era demasiado larga.
- **Dos líneas:** la carpeta y la rama van en una segunda línea, a pedido del usuario. Claude Code
  muestra cada línea que imprime el script.

## Pendiente

- **Las dos PCs no tienen el mismo script:** el de la oficina se reescribió y tiene el fix de `git -C`.
  Si se quieren idénticos, conviene copiar el de la oficina a casa, que es el más nuevo.

Ya se resolvió lo que antes figuraba como pendiente: el usuario confirmó en la status line real que la
barra (`█`/`░`) y el naranja de 24 bits se ven bien.

## Próximos pasos

- Ideas que se ofrecieron y no se implementaron todavía:
  - **Costo de la sesión:** `cost.total_cost_usd`.
  - **Líneas agregadas y borradas:** `cost.total_lines_added` / `total_lines_removed`.
  - **Estado de git:** `*` si hay cambios sin commitear, `↑↓` para commits sin pushear o pendientes de
    traer.
  - **Rama en otro color** si es `master`.
- Si se agrega algo, hacerlo en una PC y copiar el script a la otra.

## Gotchas

- **Campos del JSON:** para ver qué campos llegan de verdad, agregar temporalmente
  `printf '%s' "$input" > ~/.claude/statusline-last-input.json` después de leer el input y esperar a que
  la status line se refresque, que pasa al mandar un mensaje. Así se descubrió `effort.level`. También
  aparecieron otros campos útiles que no se usan todavía: `session_name`, `workspace.repo`,
  `prompt_cache`, `fast_mode` y `thinking.enabled`.
- **El agente `statusline-setup` no ejecuta nada:** solo tiene Read/Edit, así que no puede probar el
  script. Hay que probarlo a mano con
  `echo '<json>' | bash ~/.claude/statusline-command.sh`. Para ver los códigos de color, agregar `| cat -v`.
- **Rutas en el JSON:** vienen con backslashes escapados (`D:\\Codigo\\...`). El script los convierte a
  `/` antes de recortar la ruta.
- **Antes de configurar en otra PC, mirar qué ya existe:** en la oficina había una status line en Python
  que no estaba registrada en ningún handoff. Hay que revisar `~/.claude/` y el `statusLine` del
  `settings.json` antes de escribir.
