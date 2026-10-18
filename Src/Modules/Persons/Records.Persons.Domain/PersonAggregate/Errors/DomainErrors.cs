using BuildingBlocks.Domain.Models;
using My.Exceptions;

namespace Records.Persons.Domain.PersonAggregate.Errors;

/// <summary>
/// Contiene los errores de dominio, agrupados por entidad. Cada <see cref="Error"/> tiene un
/// código, un mensaje y un <see cref="ErrorType"/>, del que el GlobalExceptionHandler saca el
/// status HTTP.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1034:Nested types should not be visible",
    Justification = "Las clases anidadas son solo para agrupar los errores en el IntelliSense (DomainErrors.Person.NotFound).")]
public static class DomainErrors
{
    /// <summary>
    /// Contiene los errores de dominio de la persona (<see cref="Person"/>).
    /// </summary>
    public static class Person
    {
        #region Properties

        /// <summary>Cuando la persona no existe.</summary>
        public static Error NotFound =>
            new Error("ERR.PERSON.NOTFOUND", "Person not found.", ErrorType.NotFound);

        /// <summary>Cuando el país de la dirección de la persona no es válido.</summary>
        public static Error InvalidCountry =>
            new Error("ERR.PERSON.INVALIDCOUNTRY", "Country not found, invalid.", ErrorType.Forbidden);

        #endregion
    }
}
