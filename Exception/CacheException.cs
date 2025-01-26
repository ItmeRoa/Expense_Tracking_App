namespace Personal_finance_tracker.exception;

public class CacheException : Exception
{
    public CacheException()
    {
    }

    public CacheException(string msg)
    {
    }

    public CacheException(string msg, Exception innerException)
    {
    }
}

public class CachedTokenException : CacheException
{
    public CachedTokenException()
    {
    }

    public CachedTokenException(string msg)
    {
    }

    public CachedTokenException(string msg, Exception innerException)
    {
    }
}