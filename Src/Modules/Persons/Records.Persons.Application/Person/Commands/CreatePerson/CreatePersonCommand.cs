using BuildingBlocks.Application.Cqrs;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Commands.CreatePerson;

/// <summary>
/// Representa el command para crear una nueva persona.
/// </summary>
public sealed class CreatePersonCommand : ICommand<Dto.Person>
{
    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CreatePersonCommand"/>.
    /// </summary>
    /// <param name="person">El objeto <see cref="Dto.Person"/> como datos de contenido.</param>
    public CreatePersonCommand(Dto.Person person)
    {
        Person = person;
    }

    #endregion

    #region Properties

    /// <summary>El objeto <see cref="Dto.Person"/> como datos de contenido.</summary>
    public Dto.Person Person { get; init; }

    #endregion
}
