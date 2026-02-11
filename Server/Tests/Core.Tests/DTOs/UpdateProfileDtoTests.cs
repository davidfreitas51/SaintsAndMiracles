using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs
{
    public class UpdateProfileDtoTests
    {
        private static UpdateProfileDto CreateValidDto() => new()
        {
            FirstName = "John",
            LastName = "Doe"
        };

        [Fact]
        public void Should_Pass_When_Dto_Is_Valid()
        {
            var dto = CreateValidDto();

            var results = ModelValidationHelper.Validate(dto);

            Assert.Empty(results);
        }

        // ================= FirstName =================
        [Fact]
        public void Should_Fail_When_FirstName_Is_Null()
        {
            var dto = CreateValidDto();
            dto.FirstName = null!;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateProfileDto.FirstName)));
        }

        [Fact]
        public void Should_Fail_When_FirstName_Is_Empty()
        {
            var dto = CreateValidDto();
            dto.FirstName = string.Empty;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateProfileDto.FirstName)));
        }

        [Theory]
        [InlineData("123@@@")]
        [InlineData("!@#$")]
        public void Should_Fail_When_FirstName_Is_Invalid(string invalidName)
        {
            var dto = CreateValidDto();
            dto.FirstName = invalidName;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateProfileDto.FirstName)));
        }

        // ================= LastName =================
        [Fact]
        public void Should_Fail_When_LastName_Is_Null()
        {
            var dto = CreateValidDto();
            dto.LastName = null!;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateProfileDto.LastName)));
        }

        [Fact]
        public void Should_Fail_When_LastName_Is_Empty()
        {
            var dto = CreateValidDto();
            dto.LastName = string.Empty;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateProfileDto.LastName)));
        }

        [Theory]
        [InlineData("456@@@")]
        [InlineData("!@#$")]
        public void Should_Fail_When_LastName_Is_Invalid(string invalidName)
        {
            var dto = CreateValidDto();
            dto.LastName = invalidName;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateProfileDto.LastName)));
        }
    }
}
