using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

public class SafeEmailAttributeTests
{
    private class TestDto
    {
        [SafeEmail]
        public string? Email { get; set; }
    }

    [Theory]
    [InlineData("john@example.com")]
    [InlineData("jose.silva@dominio.com.br")]
    [InlineData("user+tag@sub.domain.org")]
    public void Should_Pass_When_Email_Is_Valid(string validEmail)
    {
        var dto = new TestDto { Email = validEmail };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_When_Email_IsNullOrEmpty(string? invalidEmail)
    {
        var dto = new TestDto { Email = invalidEmail };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Email), results[0].MemberNames);
        Assert.Contains("cannot be null or empty", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Email_Exceeds_MaxLength()
    {
        var dto = new TestDto { Email = new string('a', 250) + "@domain.com" }; // >254 chars

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Email), results[0].MemberNames);
        Assert.Contains("must not exceed", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Email_Has_Control_Chars()
    {
        var dto = new TestDto { Email = "john\u0001@test.com" };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Email), results[0].MemberNames);
        Assert.Contains("invalid control characters", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Email_Has_Spaces()
    {
        var dto = new TestDto { Email = "john doe@test.com" };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Email), results[0].MemberNames);
        Assert.Contains("must not contain spaces", results[0].ErrorMessage);
    }

    [Theory]
    [InlineData("john<doe@test.com")]
    [InlineData("john>doe@test.com")]
    [InlineData("john&doe@test.com")]
    [InlineData("john\"doe@test.com")]
    [InlineData("john'doe@test.com")]
    public void Should_Fail_When_Email_Has_Unsafe_Characters(string unsafeEmail)
    {
        var dto = new TestDto { Email = unsafeEmail };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Email), results[0].MemberNames);
        Assert.Contains("contains unsafe characters", results[0].ErrorMessage);
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("missing@domain")]
    [InlineData("missing@dot.")]
    [InlineData("missing@.com")]
    public void Should_Fail_When_Email_Is_Invalid_Format(string invalidEmail)
    {
        var dto = new TestDto { Email = invalidEmail };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Email), results[0].MemberNames);
        Assert.Contains("is not a valid email address", results[0].ErrorMessage);
    }
}
