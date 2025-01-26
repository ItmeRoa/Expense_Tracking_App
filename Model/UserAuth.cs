
namespace expense_tracker.Model;

public  class UserAuth
{
    public int UserAuthId { get; set; }

    public int UserId { get; set; }

    public string? AuthProvider { get; set; }

    public string? ProviderUserId { get; set; }

    public string? PasswordHashed { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
