namespace ChatClient.Configuration;

public class TrimSanitizer : ISanitizer
{
    public string Sanitize(string value)
    {
        return value.Trim();
    }
}