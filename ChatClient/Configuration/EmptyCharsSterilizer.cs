namespace ChatClient.Configuration;

public class EmptyCharsSterilizer : ISterilizer
{
    public string Sterilize(string value)
    {
        return value.Replace(" ", "");
    }
}