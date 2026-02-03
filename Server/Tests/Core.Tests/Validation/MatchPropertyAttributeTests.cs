using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

public class MatchPropertyAttributeTests
{
    private class ValidModel
    {
        public string Password { get; set; } = "Secret123!";

        [MatchProperty("Password")]
        public string ConfirmPassword { get; set; } = "Secret123!";
    }

    private class InvalidModel
    {
        public string Password { get; set; } = "Secret123!";

        [MatchProperty("Password")]
        public string ConfirmPassword { get; set; } = "WrongPassword";
    }

    private class InvalidReferenceModel
    {
        [MatchProperty("DoesNotExist")]
        public string Field { get; set; } = "Value";
    }

    private class NullPropertyModel
    {
        public string? Password { get; set; }

        [MatchProperty("Password")]
        public string? ConfirmPassword { get; set; }
    }

    [Fact]
    public void Should_Pass_When_Values_Match()
    {
        var model = new ValidModel();

        var results = ModelValidationHelper.Validate(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Values_Do_Not_Match()
    {
        var model = new InvalidModel();

        var results = ModelValidationHelper.Validate(model);

        Assert.Single(results);
        Assert.Equal("ConfirmPassword must match Password.", results[0].ErrorMessage);
        Assert.Contains(nameof(InvalidModel.ConfirmPassword), results[0].MemberNames);
    }

    [Fact]
    public void Should_Fail_When_Referenced_Property_Does_Not_Exist()
    {
        var model = new InvalidReferenceModel();

        var results = ModelValidationHelper.Validate(model);

        Assert.Single(results);
        Assert.Equal("Unknown property 'DoesNotExist'.", results[0].ErrorMessage);
        Assert.Contains(nameof(InvalidReferenceModel.Field), results[0].MemberNames);
    }

    [Fact]
    public void Should_Pass_When_Property_Is_Null()
    {
        var model = new NullPropertyModel
        {
            Password = null,
            ConfirmPassword = null
        };

        var results = ModelValidationHelper.Validate(model);

        Assert.Empty(results);
    }
}
