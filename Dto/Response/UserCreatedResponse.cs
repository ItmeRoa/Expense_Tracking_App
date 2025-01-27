namespace expense_tracker.Dto.Response;

public class UserCreatedResponse : ResponseMsg
{
    public required UserMetaData MetaData { get; set; }
    public required string Token { get; set; }
}

public  class UserMetaData
{
    public required int UserId { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
}