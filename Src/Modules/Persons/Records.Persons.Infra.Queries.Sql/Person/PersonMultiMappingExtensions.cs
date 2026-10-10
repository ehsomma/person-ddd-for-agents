using System.Data.Common;
using Dapper;
using Dto = Records.Persons.Dtos.Person; // Using aliases.

namespace Records.Persons.Infra.Queries.Sql.Person;

/// <summary>
/// Extension methods to read <see cref="Dto.Person"/> DTOs (with their address and personal assets) with the
/// Dapper multi-mapping, shared by all the person query repositories.
/// </summary>
internal static class PersonMultiMappingExtensions
{
    #region Public methods

    /// <summary>
    /// Executes the <paramref name="command"/> and maps each row directly to the DTOs, grouping the rows of the
    /// same person (one row per personal asset).
    /// </summary>
    /// <remarks>
    /// The SELECT must return the columns in this order (it defines the splitOn "Id,Lat,Id"):
    /// <list type="number">
    /// <item><c>PE.[Id], PE.[FullName], PE.[Email], PE.[Phone], PE.[Gender], PE.[Birthdate]</c> (Person).</item>
    /// <item><c>AD.[Id], AD.[StreetLine1], AD.[StreetLine2], AD.[City], AD.[State], AD.[Country]</c> (Address, from AD.Id).</item>
    /// <item><c>AD.[Lat], AD.[Lng]</c> (LatLng, from AD.Lat).</item>
    /// <item><c>PA.[Id], PA.[Description], PA.[Value]</c> (PersonalAsset, from PA.Id).</item>
    /// </list>
    /// The persons keep the order of the SELECT (sort it with ORDER BY).
    /// </remarks>
    /// <param name="connection">The connection to the database.</param>
    /// <param name="command">The command with the SELECT, its parameters and the cancellation token.</param>
    /// <returns>The persons read (empty if there are none).</returns>
    public static async Task<IList<Dto.Person>> QueryPersonsAsync(this DbConnection connection, CommandDefinition command)
    {
        // Dapper returns null for a split object when its first column is NULL: no address (AD.Id), no
        // coordinates (AD.Lat) or no personal assets (PA.Id).
        IEnumerable<(Dto.Person Person, Dto.PersonalAsset? PersonalAsset)> rows = await connection.QueryAsync<
            Dto.Person,
            Dto.Address?,
            Dto.LatLng?,
            Dto.PersonalAsset?,
            (Dto.Person, Dto.PersonalAsset?)>(
            command,
            (person, address, latLng, personalAsset) =>
            {
                address?.LatLng = latLng;
                person.Address = address;

                return (person, personalAsset);
            },
            splitOn: "Id,Lat,Id");

        // The rows of the same person have the same person and address, only the personal asset changes
        // (GroupBy keeps the order of the first appearance of each person).
        IList<Dto.Person> persons = rows
            .GroupBy(row => row.Person.Id)
            .Select(group =>
            {
                Dto.Person person = group.First().Person;
                person.PersonalAssets = group
                    .Select(row => row.PersonalAsset)
                    .OfType<Dto.PersonalAsset>() // Discards the null (person without personal assets).
                    .ToList();

                return person;
            })
            .ToList();

        return persons;
    }

    #endregion
}
