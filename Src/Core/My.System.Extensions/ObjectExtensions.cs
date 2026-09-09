using System.Text.Json;

#pragma warning disable IDE0130 // Namespace does not match folder structure
//// ReSharper disable once CheckNamespace
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extension method for object.
/// </summary>
public static class ObjectExtensions
{
    // Se instancia una sola vez y se reutiliza en toda la clase.
    // NOTE: Field con asignación inline: se inicializa directamente junto con su declaración,
    // en vez de hacerlo en un constructor.
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Converts the value of a type into a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A JSON string representation of the value.</returns>
    public static string ToJson<T>(this T obj)
    {
        return JsonSerializer.Serialize(obj, _serializerOptions);
    }
}
