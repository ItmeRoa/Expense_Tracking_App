namespace Personal_finance_tracker.exception;

public class ServiceException(string message, Exception innerException) : Exception(message, innerException);