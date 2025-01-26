namespace expense_tracker.Exception;

public class ServiceException(string message, System.Exception innerException) : System.Exception(message, innerException);