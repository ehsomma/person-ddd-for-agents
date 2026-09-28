using Records.Persons.Domain.PersonAggregate.Models;

namespace Records.Persons.Domain.PersonAggregate.Repositories;

/// <summary>
/// Defines the repository for <see cref="Person"/>.
/// </summary>
public interface IPersonRepository
{
    /// <summary>
    /// Inserts the specified person into the repository.
    /// </summary>
    /// <param name="person">The Person to insert.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task InsertAsync(Person person);

    /// <summary>
    /// Update the specified person in the repository.
    /// </summary>
    /// <param name="person">The Person to update.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpdateAsync(Person person);

    /// <summary>
    /// Deletes the specified person from the repository.
    /// </summary>
    /// <param name="person">The Person to delete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DeleteAsync(Person person);

    /// <summary>
    /// Get the <see cref="Person"/> corresponding to the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The identification to search for.</param>
    /// <returns>The corresponding Person.</returns>
    Task<Person?> GetByIdAsync(Guid id);
}
