public class NullValidator : IValidator
{
    public bool Validate(string value)
    {
        return true;
    }
}