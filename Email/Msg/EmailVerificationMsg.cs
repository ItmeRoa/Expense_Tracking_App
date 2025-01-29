namespace expense_tracker.Email.Msg;

public class EmailVerificationMsg
{
    public uint VerificationCode { get; set; }
    public required string ReceiverEmail { get; set; }
    public int VerificationExpTime { get; set; }
}