using BuildingBlocks.Infra.Persistence;
using Dapper.Contrib.Extensions;

namespace Records.Persons.Infra.Persistence.Sql.PersonAggregate.Models;

/// <summary>
/// Represents the Person data model for table Persons.
/// </summary>
[Table("Persons")]
public class Person : EntityAuditable
{
    #region Properties

    /// <summary>The Person ID.</summary>
    ////[Key]
    [ExplicitKey] // For GUIDs (Dapper.Contrib).
    public Guid Id { get; init; }

    /// <summary>The <see cref="Address"/> of the person.</summary>
    [Write(false)]
    public Address? Address { get; set; }

    /// <summary>The full name.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>The email.</summary>
    public string? Email { get; init; }

    /// <summary>The phone number.</summary>
    public string? Phone { get; init; }

    /// <summary>The gender ("Male, Female, Other").</summary>
    public string? Gender { get; init; }

    /// <summary>The birthdate.</summary>
    public DateTime? Birthdate { get; init; }

    /// <summary>The <see cref="PersonalAsset"/> list.</summary>
    [Write(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage",
        "CA2227:Collection properties should be read only",
        Justification = "We use it in mapper.")]
    public IList<PersonalAsset>? PersonalAssets { get; set; } = new List<PersonalAsset>();

    #endregion
}
