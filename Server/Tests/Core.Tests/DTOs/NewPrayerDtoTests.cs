using Core.DTOs;
using Xunit;
using System.Collections.Generic;

namespace Core.Tests.DTOs
{
    public class NewPrayerDtoTests
    {
        private static NewPrayerDto CreateValidDto() => new()
        {
            Title = "Valid Title",
            Description = "Valid description",
            MarkdownContent = "Some **markdown** content",
            Image = "prayers/image.jpg",
        };

        [Fact]
        public void Should_Pass_When_Dto_Is_Valid()
        {
            var dto = CreateValidDto();

            var results = ModelValidationHelper.Validate(dto);

            Assert.Empty(results);
        }

        // ================= Title =================
        [Fact]
        public void Should_Fail_When_Title_Is_Null()
        {
            var dto = CreateValidDto();
            dto.Title = null!;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(NewPrayerDto.Title)));
        }

        [Fact]
        public void Should_Fail_When_Title_Is_Empty()
        {
            var dto = CreateValidDto();
            dto.Title = string.Empty;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(NewPrayerDto.Title)));
        }

        // ================= Description =================
        [Fact]
        public void Should_Fail_When_Description_Is_Null()
        {
            var dto = CreateValidDto();
            dto.Description = null!;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(NewPrayerDto.Description)));
        }

        [Fact]
        public void Should_Fail_When_Description_Is_Empty()
        {
            var dto = CreateValidDto();
            dto.Description = string.Empty;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(NewPrayerDto.Description)));
        }

        // ================= MarkdownContent =================
        [Fact]
        public void Should_Fail_When_MarkdownContent_Is_Null()
        {
            var dto = CreateValidDto();
            dto.MarkdownContent = null!;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(NewPrayerDto.MarkdownContent)));
        }

        [Fact]
        public void Should_Fail_When_MarkdownContent_Is_Empty()
        {
            var dto = CreateValidDto();
            dto.MarkdownContent = string.Empty;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(NewPrayerDto.MarkdownContent)));
        }

        // ================= TagIds =================
        [Fact]
        public void Should_Fail_When_TagIds_Exceed_MaxItems()
        {
            var dto = CreateValidDto();
            dto.TagIds = new List<int> { 1, 2, 3, 4, 5, 6 }; // 6 itens

            var results = ModelValidationHelper.Validate(dto);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(NewPrayerDto.TagIds)));
        }

        // ================= Image =================
        [Fact]
        public void Should_Pass_When_Image_Is_Null()
        {
            var dto = CreateValidDto();
            dto.Image = null;

            var results = ModelValidationHelper.Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void Should_Pass_When_Image_Is_ValidPath()
        {
            var dto = CreateValidDto();
            dto.Image = "images/prayer.png";

            var results = ModelValidationHelper.Validate(dto);

            Assert.Empty(results);
        }
    }
}
