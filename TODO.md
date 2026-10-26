* [X] Documentación xml de endpoints/Scalar.
* [X] Validación con fluentvalidation de endpoints.
* [X] Return con espacio.
* [ ] Outbox.
* [ ] Worker de background.
* [X] Columna límite para comentarios VS 100 vs Claude 120?
* [X] Estandar sobre comentarios e idioma de comentarios.
* [X] Skill para casos de uso nuevos.
* [ ] AppId: Si se quiere obtener el AppId (o AppKey), tomarlo de negocio con la propiedad Source y que valide con negocio.
      Si se quiere logear, se puede usar el Client_Id que se obtiene del token de autenticación.
* [ ] Test unitarios.
* [*] CI.
* [ ] Analizar skill /refactor de https://github.com/BrighterCommand/Brighter/tree/master/.claude/commands/refactor
* [ ] Analizar este repo: https://github.com/BrighterCommand/Brighter/blob/master/README.md
* [ ] Si se requiere usar un httpclient para leer un api externa, ver ejemplo de DDD Records.Countries.Infra.Client, ahí usa httpclient inyectado y Polly.
        En lugar de usar Polly analizar Fances (mantienen una copia de Polly libre): https://github.com/BrighterCommand/fences
* [ ] ...