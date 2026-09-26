using Records.Shared.Application.Cqrs;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Commands.CreatePerson;

/// <summary>
/// Represents a command handler for creating a new person..
/// </summary>
internal sealed class CreatePersonCommandHandler : CommandHandler<CreatePersonCommand, Dto.Person>
{
    #region Public methods

    /// <inheritdoc />
    public override Task<Dto.Person> Handle(CreatePersonCommand command, CancellationToken cancellationToken = default)
    {
        // TODO: Crear clase base Shared.Application.CammandHandler (tomar ejemplo del proyecto DDD).
        return Task.FromResult(command.Person);
    }

    #endregion
}
