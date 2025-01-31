namespace expense_tracker.Dto.Response;

public class SignupUapResponse : ResponseMsg
{
    public required Guid Session { get; set; }
    public required string SignUpSessionExpAt { get; set; }
}