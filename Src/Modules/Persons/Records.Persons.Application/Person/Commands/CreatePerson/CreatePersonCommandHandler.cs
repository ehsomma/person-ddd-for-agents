using Records.Shared.Cqrs.Abstractions;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Application.Person.Commands.CreatePerson;

/// <summary>
/// Represents a command handler for creating a new person..
/// </summary>
internal sealed class CreatePersonCommandHandler : ICommandHandler<CreatePersonCommand, Dto.Person>
{
    /// <inheritdoc />
    public Task<Dto.Person> Handle(CreatePersonCommand command, CancellationToken cancellation = default)
    {
        // TODO: Crear clase base Shared.Application.CammandHandler (tomar ejemplo del proyecto DDD).
        return Task.FromResult(command.Person);
    }
}
