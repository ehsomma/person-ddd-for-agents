using BuildingBlocks.Domain.Models;

namespace BuildingBlocks.Domain.UnitTests.TestData;

/// <summary>
/// A minimal <see cref="Enumeration{TEnum}"/>, declared like the enumerations of the modules
/// (e.g. <c>Gender</c>).
/// </summary>
internal sealed class TestColor : Enumeration<TestColor>
{
    #region Declarations

    /// <summary>The red color.</summary>
    public static readonly TestColor Red = new TestColor(1, nameof(Red));

    /// <summary>The green color.</summary>
    public static readonly TestColor Green = new TestColor(2, nameof(Green));

    /// <summary>The blue color.</summary>
    public static readonly TestColor Blue = new TestColor(3, nameof(Blue));

    #endregion

    #region Constructor

    /// <summary>Initializes a new instance of the <see cref="TestColor"/> class.</summary>
    /// <param name="value">The value.</param>
    /// <param name="name">The name.</param>
    private TestColor(int value, string name)
        : base(value, name)
    {
    }

    #endregion
}
