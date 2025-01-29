namespace expense_tracker.Dto.Payload;

public class RedisEmailUapPayload
{
    public required string Email { get; set; }
    public required string Password { get; set; }

    public required bool IsEmailVerified { get; set; }
}