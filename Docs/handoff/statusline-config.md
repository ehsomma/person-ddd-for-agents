# Handoff: statusline-config

## Objetivo

Configurar la status line de Claude Code. Es una tarea de configuración del entorno: **no se tocó ningún
archivo del repo**, salvo este handoff.

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

- **`C:\Users\Esteban\.claude\settings.json`:** se agregó
  `"statusLine": {"type": "command", "command": "bash ~/.claude/statusline-command.sh"}`. El resto de
  las keys quedó igual.
- **`C:\Users\Esteban\.claude\statusline-command.sh`:** script nuevo que lee el JSON de stdin e imprime la
  línea.

### Decisiones

- **Sin `jq`:** no se verificó si está instalado en Windows. El script parsea el JSON solo con
  bash/sed/grep para no depender de él.
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
- **Barra de contexto:** sale de `context_window.used_percentage`, se toma el primer valor después de
  `"context_window"` para no agarrar el de `rate_limits`.
  - Tiene 10 casilleros, cada uno equivale al 10% y se redondea (`(pct+5)/10`).
  - La barra y el % van del mismo color: verde por debajo del 50%, amarillo por debajo del 80% y rojo
    desde el 80%. Antes el % salía en gris y el usuario pidió unificarlo.
- **Límites del plan (plan Team):** salen de `rate_limits.five_hour` y `rate_limits.seven_day`, cada uno
  con `used_percentage` y `resets_at`.
  - `resets_at` es un timestamp Unix y se pasa a hora local con `date -d @<ts>`. Se verificó que la zona
    horaria de Git Bash coincide con la de Windows (UTC-3).
  - Los % usan los mismos colores que el contexto. Si falta algún límite, no se muestra.
  - Los nombres de los días (`dom`…`sáb`) están en un array del script para no depender de la
    configuración de idioma de Git Bash.
  - Se eligió la opción B (hora y día exactos del reset). Se descartaron dos alternativas:
    - **A, tiempo restante con `↻4h12m`:** al usuario el ícono se le superponía con el dato.
    - **C, con mini barras:** era demasiado larga.
- **Dos líneas:** la carpeta y la rama van en una segunda línea, a pedido del usuario. Claude Code
  muestra cada línea que imprime el script.

## Pendiente

- **Barra sin confirmar en la terminal real:** no está confirmado que los caracteres `█` y `░` se vean
  bien en la status line real de Claude Code. Se probó a mano en Git Bash.
- **Naranja sin confirmar en la terminal real:** usa color de 24 bits (`38;2;217;119;87`). Si la
  terminal no lo soporta, cambiar la variable `orange` del script por el color 173 de la paleta de 256
  colores (`38;5;173`).

## Próximos pasos

- Si los bloques se ven como `?` o cuadraditos, cambiar la barra a ASCII (`[#####-----]`) en el bloque
  `# Mini progress bar` del script.
- Ideas que se ofrecieron y no se implementaron todavía:
  - **Costo de la sesión:** `cost.total_cost_usd`.
  - **Líneas agregadas y borradas:** `cost.total_lines_added` / `total_lines_removed`.
  - **Estado de git:** `*` si hay cambios sin commitear, `↑↓` para commits sin pushear o pendientes de
    traer.
  - **Rama en otro color** si es `master`.

## Gotchas

- **Campos del JSON:** para ver qué campos llegan de verdad, agregar temporalmente
  `printf '%s' "$input" > ~/.claude/statusline-last-input.json` después de leer el input y esperar a que
  la status line se refresque, que pasa al mandar un mensaje. Así se descubrió `effort.level`. También
  aparecieron otros campos útiles que no se usan todavía: `session_name`, `workspace.repo`,
  `prompt_cache`, `fast_mode` y `thinking.enabled`.
- **El agente `statusline-setup` no ejecuta nada:** solo tiene Read/Edit, así que no puede probar el
  script. Hay que probarlo a mano con
  `echo '<json>' | bash ~/.claude/statusline-command.sh`.
- **Rutas en el JSON:** vienen con backslashes escapados (`D:\\Codigo\\...`). El script los convierte a
  `/` antes de recortar la ruta.
