namespace expense_tracker.Dto.Request.UserRequest;

public class SignUpByUapRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}