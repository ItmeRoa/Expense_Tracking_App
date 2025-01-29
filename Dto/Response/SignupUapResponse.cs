namespace expense_tracker.Dto.Response;

public class SignupUapResponse : ResponseMsg
{
    public required Guid Session { get; set; }
    public required DateTime SignUpSessionExpAt { get; set; }
}