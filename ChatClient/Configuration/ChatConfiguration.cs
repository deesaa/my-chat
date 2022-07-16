using ChatClient.Configuration;

namespace ChatClient;

public class ChatConfiguration
{
    private readonly Dictionary<string, IValidator> _argValidators = new();
    private readonly Dictionary<string, ISterilizer> _argSterilizers = new();

    public void SetValidator(string argName, IValidator validator)
    {
        _argValidators[argName] = validator;
    }
    
    public void SetSterilizer(string argName, ISterilizer sterilizer)
    {
        _argSterilizers[argName] = sterilizer;
    }

    public bool SterilizeValidate(string value, string argName, out string outname)
    {
        bool isValid = true;
        if (_argSterilizers.ContainsKey(argName))
            value = _argSterilizers[argName].Sterilize(value);
        else
            Console.WriteLine($"WARNING: There is no Sterilizer for arg {argName}");
        
        if (_argValidators.ContainsKey(argName))
            isValid = _argValidators[argName].Validate(value);
        else
            Console.WriteLine($"WARNING: There is no Validator for arg {argName}");

        outname = value;
        return isValid;
    }
}