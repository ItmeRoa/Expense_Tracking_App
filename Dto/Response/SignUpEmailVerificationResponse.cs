namespace expense_tracker.Dto.Response;

public class SignUpEmailVerificationResponse : ResponseMsg
{
    public required Guid Session { get; set; }
    public required DateTime ExpiredAt { get; set; }
}