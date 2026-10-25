namespace BuildingBlocks.Domain.Models;

/// <summary>
/// Represents the base class all value objects derive from.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    #region Operators

    /// <summary>
    /// Implicit operator.
    /// </summary>
    /// <param name="left">The left object.</param>
    /// <param name="right">The right object.</param>
    /// <returns>True, if they are equals.</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Implicit operator.
    /// </summary>
    /// <param name="left">The left object.</param>
    /// <param name="right">The right object.</param>
    /// <returns>True, if they are not equals.</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Returns a boolean indicating if the passed in `other` is Equal to this. Equality is
    /// defined as object equality for reference types and bitwise equality for value types using
    /// a loader trick to replace Equals with EqualsValue for value types.
    /// </summary>
    /// <param name="other">The other object to compare.</param>
    /// <returns>True, if the specified valueObject is equal to the current valueObject. Otherwise, false.</returns>
    public bool Equals(ValueObject? other)
    {
        // NOTE: is not null vs != null. The only difference (besides the syntax) is, that the
        // compiler guarantees that no user-overloaded operator is called when using is not null
        // instead of != null (or is null instead of == null).
        return other is not null && ValuesEqual(other);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is ValueObject other && ValuesEqual(other);
    }

    /// <summary>
    /// Creates a shallow copy of the current object.
    /// </summary>
    /// <returns>A shallow copy of the current object.</returns>
    public ValueObject? GetCopy()
    {
        ValueObject? copy = MemberwiseClone() as ValueObject;

        return copy;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hashCode = default;

        foreach (object obj in GetEqualityComponents())
        {
            hashCode.Add(obj);
        }

        return hashCode.ToHashCode();
    }

    #endregion

    #region Protected methods

    /// <summary>
    /// Gets the equality components of the value object.
    /// </summary>
    /// <returns>The collection of objects representing the value object equality components.</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    #endregion

    #region Private methods

    /// <summary>
    /// Checks values of the specified ValueObject are equal to the current valueObject.
    /// </summary>
    /// <param name="other">The valueObject to compare.</param>
    /// <returns>True whether the two valueObjects are of the same type and their values are equals.</returns>
    private bool ValuesEqual(ValueObject other)
    {
        // Primero el tipo: sin esto, dos value objects distintos con el mismo valor serían iguales
        // (los componentes de igualdad son solo los valores), igual que ya hace Enumeration.
        //   Email("a@b.com") vs Email("a@b.com")        → iguales ✓
        //   Email("a@b.com") vs PhoneNumber("a@b.com")  → distintos ✓ (antes daba iguales)
        bool areEqual = GetType() == other.GetType()
            && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

        return areEqual;
    }

    #endregion
}
