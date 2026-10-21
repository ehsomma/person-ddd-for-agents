---
description: Responde una pregunta sobre el proyecto sin modificar nada (solo pregunta/respuesta).
argument-hint: [pregunta]
allowed-tools: Read, Grep, Glob, Bash(git log:*), Bash(git show:*), Bash(git diff:*), Bash(git status:*), Bash(git blame:*), Bash(dotnet build:*)
---

$ARGUMENTS

Es solo una pregunta: respondela investigando el proyecto todo lo que haga falta.

- No modifiques, crees ni borres archivos, ni ejecutes comandos que cambien el estado (run, commits, etc.).
- Podés compilar (`dotnet build`) si hace falta para comprobar algo de la respuesta.
- No armes un plan ni propongas implementar nada, salvo que te lo pida.
- Si para responder bien necesitarías cambiar algo o ejecutar algo más que lecturas, decímelo y esperá.
