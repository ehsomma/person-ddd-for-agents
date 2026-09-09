#pragma warning disable IDE0130 // Namespace does not match folder structure
// ReSharper disable once CheckNamespace
namespace System.Collections.Generic;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extension method for Collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Returns the same sequence, or an empty one if it is null, so it can be
    /// iterated with foreach without a null check.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">The IEnumerable.</param>
    /// <returns>The same IEnumerable or an empty IEnumerable if it is null.</returns>
    public static IEnumerable<T> NotNull<T>(this IEnumerable<T>? source)
    {
        return source ?? [];
    }
}
