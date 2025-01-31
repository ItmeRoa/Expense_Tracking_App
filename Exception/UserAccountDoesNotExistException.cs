namespace expense_tracker.Exception;

public class UserAccountDoesNotExistException : System.Exception
{
    public UserAccountDoesNotExistException(string msg)
    {
    }

    public UserAccountDoesNotExistException(string email, string msg)
    {
    }
}