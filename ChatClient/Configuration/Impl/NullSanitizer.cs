namespace ChatClient.Configuration;

public class NullSanitizer : ISanitizer
{
    public string Sanitize(string value)
    {
        return value;
    }
}