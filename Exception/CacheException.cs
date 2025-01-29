namespace expense_tracker.Exception;

public class CacheException : System.Exception
{
    public CacheException()
    {
    }

    public CacheException(string msg)
    {
    }

    public CacheException(string msg, System.Exception innerException)
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

    public CachedTokenException(string msg, System.Exception innerException)
    {
    }
}