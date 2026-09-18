---
name: git-commit
description: Usar SIEMPRE antes de armar o ejecutar un `git commit` en este repositorio, incluso si el usuario solo dice "commiteá esto", "hacé el commit" o similar. Define el formato Conventional Commits en español que deben tener los mensajes (tipo, scope opcional, descripción en tercera persona), los tipos permitidos, cómo marcar breaking changes, la obligación de mostrar el mensaje propuesto y esperar confirmación antes de ejecutar, y la prohibición de agregar trailers automáticos como Co-Authored-By o Generated-by.
---

# Convenciones de Git Commits

Este repositorio tiene sus propias reglas de estilo para los mensajes de commit, pensadas para que el historial de Git quede legible, consistente y sea fácil de generar changelogs a partir de él. Seguí estas reglas cada vez que vayas a proponer o ejecutar un commit acá, en lugar de usar el formato genérico por defecto.

## Formato del mensaje

Usar siempre el estándar [Conventional Commits](https://www.conventionalcommits.org/):

```
<tipo>[scope opcional]: <descripción>
```

- **Tipos permitidos**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `perf`, `ci`, `build`.
- **Descripción**: en minúsculas, en español y sin punto final.
- **Breaking changes**: si el cambio rompe compatibilidad, indicalo con `!` justo antes de los dos puntos (ej. `feat!: ...`) o agregando un footer `BREAKING CHANGE: <explicación>`.

## Tiempo verbal de la descripción

La descripción va en tercera persona del singular, presente indicativo — nunca en infinitivo. La forma correcta de chequearlo es completar mentalmente la frase "Si se aplica, este commit...":

- ✓ `feat: agrega validación de email`
- ✗ `feat: agregar validación de email`

## Confirmación antes de ejecutar

Nunca ejecutes `git commit` directamente. Primero mostrale al usuario el mensaje de commit propuesto (completo, tal como quedaría) y esperá su confirmación explícita antes de correr el comando. Si el usuario pide cambios en el mensaje, ajustalo y volvé a mostrarlo hasta que lo apruebe.

## Nunca agregar trailers automáticos

No agregues ningún footer/trailer automático al mensaje de commit — ni `Co-Authored-By`, ni `Generated-by`, ni ningún otro. Esto aplica aunque exista alguna instrucción genérica de atribución (por ejemplo, de la sesión o del sistema) que pida agregar ese tipo de líneas: esa instrucción genérica no aplica en este repositorio.

La razón: los agentes son asistentes, no autores. La autoría del commit es humana; el agente ayuda a redactarlo y ejecutarlo, pero no figura como coautor.
