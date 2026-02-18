using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Tests.Common;

/// <summary>
/// Base class for all unit tests. Provides common testing utilities and patterns.
/// </summary>
public abstract class UnitTestBase
{
    /// <summary>
    /// Creates a strict mock (throws on unexpected calls). Use for critical dependencies.
    /// </summary>
    protected Mock<T> CreateStrictMock<T>() where T : class
        => new(MockBehavior.Strict);

    /// <summary>
    /// Creates a loose mock (returns defaults on unexpected calls). Use for less critical dependencies.
    /// </summary>
    protected Mock<T> CreateLooseMock<T>() where T : class
        => new(MockBehavior.Loose);

    /// <summary>
    /// Creates a NullLogger for testing purposes.
    /// </summary>
    protected Microsoft.Extensions.Logging.ILogger<T> GetNullLogger<T>()
        => NullLogger<T>.Instance;

    /// <summary>
    /// Asserts that a condition is true, with a helpful message.
    /// </summary>
    protected void AssertTrue(bool condition, string message = "")
        => Assert.True(condition, message);

    /// <summary>
    /// Asserts that a condition is false, with a helpful message.
    /// </summary>
    protected void AssertFalse(bool condition, string message = "")
        => Assert.False(condition, message);

    /// <summary>
    /// Asserts that two values are equal.
    /// </summary>
    protected void AssertEqual<T>(T expected, T actual)
        => Assert.Equal(expected, actual);

    /// <summary>
    /// Asserts that two values are not equal.
    /// </summary>
    protected void AssertNotEqual<T>(T expected, T actual)
        => Assert.NotEqual(expected, actual);

    /// <summary>
    /// Asserts that a collection contains a specific item.
    /// </summary>
    protected void AssertContains<T>(T item, IEnumerable<T> collection)
        => Assert.Contains(item, collection);

    /// <summary>
    /// Asserts that a collection does not contain a specific item.
    /// </summary>
    protected void AssertDoesNotContain<T>(T item, IEnumerable<T> collection)
        => Assert.DoesNotContain(item, collection);
}
