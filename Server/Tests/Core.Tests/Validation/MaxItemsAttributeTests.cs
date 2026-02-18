using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

/// <summary>
/// Tests for MaxItemsAttribute validation.
/// Ensures collections do not exceed a maximum item count.
/// </summary>
public class MaxItemsAttributeTests : ValidationTestBase
{
    private class TestModel
    {
        [MaxItems(3)]
        public List<int>? Numbers { get; set; }

        [MaxItems(5)]
        public List<string>? Strings { get; set; }

        [MaxItems(2)]
        public int[]? Array { get; set; }

        [MaxItems(1)]
        public HashSet<string>? HashSet { get; set; }

        [MaxItems(2)]
        public object? NotACollection { get; set; }
    }

    // ==================== NULL VALUES ====================

    [Fact]
    public void Should_Pass_Null()
    {
        var model = new TestModel { Numbers = null };
        
        var results = Validate(model);

        AssertValid(results);
    }

    // ==================== EMPTY COLLECTIONS ====================

    [Fact]
    public void Should_Pass_EmptyList()
    {
        var model = new TestModel { Numbers = new List<int>() };
        
        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_EmptyArray()
    {
        var model = new TestModel { Array = Array.Empty<int>() };
        
        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_EmptyHashSet()
    {
        var model = new TestModel { HashSet = new HashSet<string>() };
        
        var results = Validate(model);

        AssertValid(results);
    }

    // ==================== WITHIN LIMIT ====================

    [Fact]
    public void Should_Pass_ExactlyAtLimit()
    {
        var model = new TestModel { Numbers = new List<int> { 1, 2, 3 } };
        
        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_BelowLimit()
    {
        var model = new TestModel { Numbers = new List<int> { 1, 2 } };
        
        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_SingleItem()
    {
        var model = new TestModel { Numbers = new List<int> { 1 } };
        
        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_Array_WithinLimit()
    {
        var model = new TestModel { Array = new[] { 1, 2 } };
        
        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_HashSet_WithinLimit()
    {
        var model = new TestModel { HashSet = new HashSet<string> { "one" } };
        
        var results = Validate(model);

        AssertValid(results);
    }

    // ==================== EXCEEDS LIMIT ====================

    [Fact]
    public void Should_Fail_ExceedsLimit()
    {
        var model = new TestModel { Numbers = new List<int> { 1, 2, 3, 4 } };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Numbers), "must contain at most 3 items");
    }

    [Fact]
    public void Should_Fail_Array_ExceedsLimit()
    {
        var model = new TestModel { Array = new[] { 1, 2, 3 } };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Array), "must contain at most 2 items");
    }

    [Fact]
    public void Should_Fail_HashSet_ExceedsLimit()
    {
        var model = new TestModel { HashSet = new HashSet<string> { "one", "two" } };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.HashSet), "must contain at most 1");
    }

    [Fact]
    public void Should_Fail_ManyItems()
    {
        var model = new TestModel { Strings = Enumerable.Range(1, 10).Select(i => i.ToString()).ToList() };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Strings), "must contain at most 5 items");
    }

    // ==================== TYPE VALIDATION ====================

    [Fact]
    public void Should_Fail_NonCollectionType()
    {
        var model = new TestModel { NotACollection = new object() };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.NotACollection), "must be a collection");
    }

    // Note: String is IEnumerable<char>, so it's treated as a collection
    [Fact]
    public void Should_Pass_StringAsCollection()
    {
        var model = new TestModel { NotACollection = "a" }; // 1 char, under limit of 2
        
        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Fail_IntegerAsCollection()
    {
        var model = new TestModel { NotACollection = 123 };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.NotACollection), "must be a collection");
    }
}
