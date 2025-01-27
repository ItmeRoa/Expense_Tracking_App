namespace expense_tracker.Dto.Response;

public class SignupUapResponse : ResponseMsg
{
    public required DateTime SignUpSessionExpAt { get; set; }
}