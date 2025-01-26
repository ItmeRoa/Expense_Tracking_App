namespace Personal_finance_tracker.exception;

public class DomainException(string? message) : Exception(message);

public class EntityAlreadyExistException(string? message) : DomainException(message);

public class EntityNotFoundException(Type entity, object id)
    : DomainException($"{nameof(entity.Name)} with ID {id} not found.");