namespace ChatServer;

public static class LinqExt
{
    public static bool TryFirst<T>(this IEnumerable<T> enumerable, out T value, Func<T, bool> predicate)
    {
        try
        {
            value = enumerable.First(predicate);
            return true;
        }
        catch (Exception e)
        {
            value = default;
            return false;
        }
    }
}