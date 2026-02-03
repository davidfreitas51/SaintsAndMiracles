using Core.Validation.Attributes;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Core.Tests.Validation;

public class MatchPropertyAttributeTests
{
    [Fact]
    public void Should_BeValid_When_ValuesMatch()
    {
        var model = new MatchPropertyTestModel
        {
            Password = "123456",
            ConfirmPassword = "123456"
        };

        var results = ModelValidationHelper.Validate(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_ValuesDoNotMatch()
    {
        var model = new MatchPropertyTestModel
        {
            Password = "123456",
            ConfirmPassword = "654321"
        };

        var results = ModelValidationHelper.Validate(model);

        Assert.Contains(
            results,
            r => r.ErrorMessage ==
                $"{nameof(MatchPropertyTestModel.ConfirmPassword)} must match {nameof(MatchPropertyTestModel.Password)}."
        );
    }


    [Fact]
    public void Should_Return_CustomErrorMessage_When_Provided()
    {
        var model = new MatchPropertyCustomMessageModel
        {
            Password = "abc",
            ConfirmPassword = "def"
        };

        var results = ModelValidationHelper.Validate(model);

        Assert.Contains(
            results,
            r => r.ErrorMessage == "Passwords do not match"
        );
    }

    [Fact]
    public void Should_Fail_When_ReferencedProperty_DoesNotExist()
    {
        var model = new MatchPropertyInvalidConfigModel();

        var results = ModelValidationHelper.Validate(model);

        Assert.Single(results);
        Assert.Equal(
            "Unknown property 'DoesNotExist'.",
            results[0].ErrorMessage
        );
    }

    private class MatchPropertyTestModel
    {
        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MatchProperty(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    private class MatchPropertyCustomMessageModel
    {
        public string Password { get; set; } = string.Empty;

        [MatchProperty(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    private class MatchPropertyInvalidConfigModel
    {
        [MatchProperty("DoesNotExist")]
        public string Value { get; set; } = "test";
    }
}
