namespace ChatClient.Configuration;

public class TrimSterilizer : ISterilizer
{
    public string Sterilize(string value)
    {
        return value.Trim();
    }
}