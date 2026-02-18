using Core.DTOs;
using Core.Models;
using Tests.Common.Builders;

namespace Tests.Common;

/// <summary>
/// Factory for creating commonly-used test data combinations.
/// This reduces duplication across tests with realistic test data.
/// </summary>
public static class TestDataFactory
{
    // --- Users ---

    public static AppUser CreateDefaultUser(string id = "user-1")
        => AppUserBuilder.Default().WithId(id).Build();

    public static AppUser CreateUserWithEmail(string email)
        => AppUserBuilder.Default().WithEmail(email).Build();

    public static AppUser CreateUnconfirmedUser(string email = "unconfirmed@test.com")
        => AppUserBuilder.Default()
            .WithEmail(email)
            .WithEmailConfirmed(false)
            .Build();

    public static AppUser CreateLockedOutUser(string id = "user-1")
        => AppUserBuilder.Default()
            .WithId(id)
            .WithLockedOut(DateTime.UtcNow.AddDays(1))
            .Build();

    // --- Login DTOs ---

    public static LoginDto CreateValidLogin(string email = "test@test.com")
        => LoginDtoBuilder.Default()
            .WithEmail(email)
            .WithPassword("ValidPassword123!")
            .Build();

    public static LoginDto CreateLoginWithInvalidEmail()
        => LoginDtoBuilder.InvalidEmail().Build();

    public static LoginDto CreateLoginWithMissingPassword()
        => LoginDtoBuilder.MissingPassword().Build();

    // --- Saint DTOs ---

    public static NewSaintDto CreateDefaultSaint()
        => NewSaintDtoBuilder.Default().Build();

    public static NewSaintDto CreateMinimalSaint()
        => NewSaintDtoBuilder.Minimal().Build();

    public static NewSaintDto CreateSaintWithName(string name)
        => NewSaintDtoBuilder.Default()
            .WithName(name)
            .Build();

    public static NewSaintDto CreateSaintWithTags(params int[] tagIds)
        => NewSaintDtoBuilder.Default()
            .WithTags(tagIds)
            .Build();

    // --- Prayer DTOs ---

    public static NewPrayerDto CreateDefaultPrayer()
        => NewPrayerDtoBuilder.Default().Build();

    public static NewPrayerDto CreateMinimalPrayer()
        => NewPrayerDtoBuilder.Minimal().Build();

    public static NewPrayerDto CreatePrayerWithTitle(string title)
        => NewPrayerDtoBuilder.Default()
            .WithTitle(title)
            .Build();

    public static NewPrayerDto CreatePrayerWithTags(params int[] tagIds)
        => NewPrayerDtoBuilder.Default()
            .WithTags(tagIds)
            .Build();

    // --- Saints (Models) ---

    public static Saint CreateSaintModel(
        int id = 1,
        string name = "Saint Francis",
        string slug = "saint-francis")
    {
        return new Saint
        {
            Id = id,
            Name = name,
            Country = "Italy",
            Century = 12,
            Image = "/saints/francis/image.webp",
            Description = "A saint",
            MarkdownPath = "/saints/francis/markdown.md",
            Slug = slug,
            Title = "The Saint",
            FeastDay = new DateOnly(1, 10, 4),
            Tags = new List<Tag>()
        };
    }

    public static Saint CreateSaintWithTags(int id = 1, params Tag[] tags)
    {
        var saint = CreateSaintModel(id);
        saint.Tags = tags.ToList();
        return saint;
    }

    // --- Tags (Models) ---

    public static Tag CreateTag(
        int id = 1,
        string name = "Saint",
        Core.Enums.TagType type = Core.Enums.TagType.Saint)
    {
        return new Tag
        {
            Id = id,
            Name = name,
            TagType = type
        };
    }

    // --- Prayers (Models) ---

    public static Prayer CreatePrayerModel(
        int id = 1,
        string title = "Hail Mary",
        string slug = "hail-mary")
    {
        return new Prayer
        {
            Id = id,
            Title = title,
            Description = "A traditional prayer",
            Image = "/prayers/hail-mary/image.webp",
            MarkdownPath = "/prayers/hail-mary/markdown.md",
            Slug = slug,
            Tags = new List<Tag>()
        };
    }

    public static Prayer CreatePrayerWithTags(int id = 1, params Tag[] tags)
    {
        var prayer = CreatePrayerModel(id);
        prayer.Tags = tags.ToList();
        return prayer;
    }
}
