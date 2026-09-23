using System.ComponentModel.DataAnnotations;

namespace Records.Shared.Configuration;

/// <summary>
/// Represents the settings that will be mapped from the SqlServer key in the appsettings.json file.
/// </summary>
public class SqlServerSettings
{
    #region Declarations

    /// <summary>The key name to map from the appsettings.json file.</summary>
    public const string SettingsKey = "SqlServer"; // Without "...Settings" suffix.

    #endregion

    #region Properties

    ////public const string SourceDatabaseConnectionStringKey = "SqlSourceDatabase";

    ////public const string ProjectionDatabaseConnectionStringKey = "SqlProjectionDatabase";

    /// <summary>Connection string for the "Source" (Write) database.</summary>
    [Required(AllowEmptyStrings = false)]
    public string SourceConnectionString { get; init; } = string.Empty;

    /// <summary>Connection string for the "Projection" (Read) database.</summary>
    [Required(AllowEmptyStrings = false)]
    public string ProjectionConnectionString { get; init; } = string.Empty;

    #endregion
}
