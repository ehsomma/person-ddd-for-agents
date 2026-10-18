using System.Diagnostics.CodeAnalysis;

namespace Records.Persons.Dtos.Person;

/// <summary>
/// Representa una persona, con su dirección y sus bienes personales.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1629:DocumentationTextMustEndWithAPeriod",
    Justification = "Para permitir omitir el punto final en el tag <example> usado por OpenAPI.")]
public class Person
{
    #region Properties

    /// <summary>ID de la persona.</summary>
    public Guid Id { get; init; }

    /// <summary>Nombre completo de la persona.</summary>
    /// <example>Doe, John</example>
    public string? FullName { get; init; }

    /// <summary>Email de la persona.</summary>
    /// <example>johndoe@gmail.com</example>
    public string? Email { get; init; }

    /// <summary>Número de teléfono (opcional).</summary>
    /// <example>1150011234</example>
    public string? Phone { get; init; }

    /// <summary>Género de la persona. Valores posibles: Male, Female u Other.</summary>
    /// <example>Male</example>
    // TODO: Implementar el enum Gender en el DTO, así OpenAPI documenta los valores posibles.
    public string? Gender { get; init; }

    /// <summary>Fecha de nacimiento (opcional).</summary>
    public DateTime? Birthdate { get; init; }

    /// <summary>Dirección de la persona (<see cref="Address"/>).</summary>
    public Address? Address { get; set; }

    /// <summary>Bienes personales de la persona (<see cref="PersonalAsset"/>).</summary>
    [SuppressMessage(
        "Usage",
        "CA2227:Collection properties should be read only",
        Justification = "Tiene set (y no init) para que los query repositories puedan armar el DTO después del multi-mapping de Dapper.")]
    public IList<PersonalAsset>? PersonalAssets { get; set; }

    #endregion
}
