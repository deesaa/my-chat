namespace ChatClient.Configuration;

public class EmptyCharsSanitizer : ISanitizer
{
    public string Sanitize(string value)
    {
        return value.Replace(" ", "");
    }
}