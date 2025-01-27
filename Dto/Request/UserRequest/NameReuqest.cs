namespace expense_tracker.Dto.Request.UserRequest;

public class NameReuqest
{
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
}