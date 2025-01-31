namespace expense_tracker.Dto.Response;

public class LoginResponse : ResponseMsg
{
    public required UserInfoLoginData UserInfo { get; set; }
    public required string Token { get; set; }
}

public class UserInfoLoginData
{
    public required int UserId { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required bool IsEmailVerified { get; set; }
    public required SubscriptionResponse SubscriptionDetails { get; set; }
}

public class SubscriptionResponse
{
    public required string UserSubscriptionPlan { get; set; }
    public required string ExpiredAt { get; set; }
    public  RemainingTime? RemainingTime { get; set; }
}

public class RemainingTime
{
    public required int Days { get; set; }
    public required int Hours { get; set; }
    public required int Minutes { get; set; }
}