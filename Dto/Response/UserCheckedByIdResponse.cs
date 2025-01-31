namespace expense_tracker.Dto.Response;

public class UserCheckedByIdResponse : ResponseMsg
{
    public required UserCheckedByIdModel UserInfo { get; set; }
}

public class UserCheckedByIdModel
{
    public required int UserId { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public required string UserSubscriptionPlan { get; set; }
    public required bool IsEmailVerified { get; set; }
    public required DateTime CreateAt { get; set; }
    public required string RemainingSubscription { get; set; }
}