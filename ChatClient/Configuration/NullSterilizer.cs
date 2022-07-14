public class NullSterilizer : ISterilizer
{
    public string Sterilize(string value)
    {
        return value;
    }
}