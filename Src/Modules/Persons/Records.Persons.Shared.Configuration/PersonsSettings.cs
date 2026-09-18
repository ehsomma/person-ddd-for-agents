#region  Usings

using System.ComponentModel.DataAnnotations;

#endregion

namespace Records.Persons.Shared.Configuration;

/// <summary>
/// Represents the settings that will be mapped from the Persons key in the appsettings.json file.
/// </summary>
public class PersonsSettings
{
    #region Declarations

    /// <summary>The key name to map from the appsettings.json file.</summary>
    public const string SettingsKey = "Persons"; // Without "...Settings" suffix.

    #endregion

    #region Properties

    /// <summary>Setting 1 (example).</summary>
    /// <remarks>Obligatoria: si falta o viene vacía, lanza excepción al cargar.</remarks>
    [Required(AllowEmptyStrings = false)]
    public string Setting1 { get; set; } = string.Empty;

    /// <summary>Setting 2 (example).</summary>
    /// <remarks>Opcional: si no está en el JSON, queda este default.</remarks>
    public string Setting2 { get; set; } = "valor 2 (default)";

    #endregion
}
