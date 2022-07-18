namespace ChatClient.Configuration;

public interface ISanitizer
{
    string Sanitize(string value);
}