namespace Personal_finance_tracker.exception;

public class RepoException(string message, Exception innerException) : Exception(message, innerException);