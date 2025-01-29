namespace expense_tracker.Exception;

public class RepoException(string message, System.Exception innerException) : System.Exception(message, innerException);