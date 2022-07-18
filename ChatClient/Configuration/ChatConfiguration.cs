using ChatClient.Configuration;

namespace ChatClient;

public class ChatConfiguration
{
    private readonly Dictionary<string, IValidator> _argValidators = new();
    private readonly Dictionary<string, ISanitizer> _argSanitizers = new();

    public void SetValidator(string argName, IValidator validator)
    {
        _argValidators[argName] = validator;
    }
    
    public void SetSanitizer(string argName, ISanitizer sanitizer)
    {
        _argSanitizers[argName] = sanitizer;
    }

    public bool SanitizeValidate(string value, string argName, out string outname)
    {
        bool isValid = true;
        if (_argSanitizers.ContainsKey(argName))
            value = _argSanitizers[argName].Sanitize(value);
        else
            Console.WriteLine($"WARNING: There is no Sanitizer for arg {argName}");
        
        if (_argValidators.ContainsKey(argName))
            isValid = _argValidators[argName].Validate(value);
        else
            Console.WriteLine($"WARNING: There is no Validator for arg {argName}");

        outname = value;
        return isValid;
    }
}