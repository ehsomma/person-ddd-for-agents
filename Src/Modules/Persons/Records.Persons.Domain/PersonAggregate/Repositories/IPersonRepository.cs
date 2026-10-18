using Records.Persons.Domain.PersonAggregate.Models;

namespace Records.Persons.Domain.PersonAggregate.Repositories;

/// <summary>
/// Define el repositorio (write side) del aggregate <see cref="Person"/>. Trabaja siempre con el
/// aggregate completo: la persona, su dirección y sus bienes personales.
/// </summary>
/// <remarks>
/// Se define en el dominio y se implementa en la infraestructura (Infra.Persistence.Sql), así el
/// dominio no depende de cómo se persiste. Las escrituras corren dentro de la transacción que abre
/// el command handler con <c>ExecuteInTransaction</c>.
/// </remarks>
public interface IPersonRepository
{
    /// <summary>
    /// Inserta la persona completa: la persona, su dirección y sus bienes personales.
    /// </summary>
    /// <param name="person">La persona a insertar.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación.</param>
    /// <returns>Una <see cref="Task"/> que representa la operación asincrónica.</returns>
    Task InsertAsync(Person person, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza la persona y su dirección, e inserta los bienes personales nuevos (los que todavía
    /// no tienen ID). Los bienes personales existentes no se modifican.
    /// </summary>
    /// <param name="person">La persona a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación.</param>
    /// <returns>Una <see cref="Task"/> que representa la operación asincrónica.</returns>
    Task UpdateAsync(Person person, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina la persona completa, con su dirección y sus bienes personales.
    /// </summary>
    /// <param name="person">La persona a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación.</param>
    /// <returns>Una <see cref="Task"/> que representa la operación asincrónica.</returns>
    Task DeleteAsync(Person person, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el aggregate completo de la persona con el <paramref name="id"/> especificado, para
    /// que un command lo modifique. Si no existe devuelve <see langword="null"/>, y el que llama
    /// decide qué hacer (p.ej. el PersonService lanza DomainErrors.Person.NotFound).
    /// </summary>
    /// <param name="id">El ID de la persona a buscar.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación.</param>
    /// <returns>La persona, o <see langword="null"/> si no existe.</returns>
    Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
