namespace ChatClient.Configuration;

public interface IValidator
{
    bool Validate(string value);
}