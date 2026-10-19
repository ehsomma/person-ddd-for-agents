using BuildingBlocks.Application.Cqrs;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Commands.UpdatePerson;

/// <summary>
/// Representa un comando para actualizar una persona existente.
/// </summary>
public sealed class UpdatePersonCommand : ICommand<Dto.Person>
{
    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="UpdatePersonCommand"/>.
    /// </summary>
    /// <param name="appKey">La appKey (solo un ejemplo).</param>
    /// <param name="person">El objeto <see cref="Dto.Person"/> como datos de contenido.</param>
    public UpdatePersonCommand(string appKey, Dto.Person person)
    {
        AppKey = appKey;
        Person = person;
    }

    #endregion

    #region Properties

    /// <summary>El objeto <see cref="Dto.Person"/> como datos de contenido.</summary>
    public Dto.Person Person { get; init; }

    /// <summary>La appKey (solo un ejemplo).</summary>
    public string AppKey { get; init; }

    #endregion
}
