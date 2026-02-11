using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs
{
    public class ResetPasswordDtoTests
    {
        private static ResetPasswordDto CreateValidDto() => new()
        {
            Email = "user@example.com",
            Token = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", // 32 chars válidos
            NewPassword = "MyNewPassword123!"
        };

        [Fact]
        public void Should_Pass_When_Dto_Is_Valid()
        {
            var dto = CreateValidDto();

            var results = ModelValidationHelper.Validate(dto);

            Assert.Empty(results);
        }

        // ================= Email =================
        [Fact]
        public void Should_Fail_When_Email_Is_Null()
        {
            var dto = CreateValidDto();
            dto.Email = null!;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResetPasswordDto.Email)));
        }

        [Fact]
        public void Should_Fail_When_Email_Is_Empty()
        {
            var dto = CreateValidDto();
            dto.Email = string.Empty;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResetPasswordDto.Email)));
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("bad@domain")]
        [InlineData("john@@example.com")]
        public void Should_Fail_When_Email_Is_Invalid(string invalidEmail)
        {
            var dto = CreateValidDto();
            dto.Email = invalidEmail;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResetPasswordDto.Email)));
        }

        // ================= Token =================
        [Fact]
        public void Should_Fail_When_Token_Is_Null()
        {
            var dto = CreateValidDto();
            dto.Token = null!;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResetPasswordDto.Token)));
        }

        [Fact]
        public void Should_Fail_When_Token_Is_Empty()
        {
            var dto = CreateValidDto();
            dto.Token = string.Empty;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResetPasswordDto.Token)));
        }

        // ================= NewPassword =================
        [Fact]
        public void Should_Fail_When_NewPassword_Is_Null()
        {
            var dto = CreateValidDto();
            dto.NewPassword = null!;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResetPasswordDto.NewPassword)));
        }

        [Fact]
        public void Should_Fail_When_NewPassword_Is_Empty()
        {
            var dto = CreateValidDto();
            dto.NewPassword = string.Empty;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResetPasswordDto.NewPassword)));
        }
    }
}
