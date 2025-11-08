namespace Core.Models;

public class AccountToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Hash { get; set; } = null!;
    public string Role { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? UsedAtUtc { get; set; }
    public bool IsUsed { get; set; }

    public string? IssuedTo { get; set; }
    public string? Purpose { get; set; }
}
