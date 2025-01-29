namespace expense_tracker.Email.Msg;

public class EmailVerificationMsg
{
    public required uint VerificationCode { get; set; }
    public required int VerificationExpTime { get; set; }
}