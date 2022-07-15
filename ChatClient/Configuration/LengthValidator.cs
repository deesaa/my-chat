namespace ChatClient.Configuration;

public class LengthValidator : IValidator
{
    private Action<string, int> _onToLongCallback;
    private Action<string, int> _onToSmallCallback;
    private int _minLength;
    private int _maxLength;

    private IValidator _previousValidator;
    
    public LengthValidator(int minLength, int maxLength, 
        Action<string, int> onToLongCallback, Action<string, int> onToSmallCallback)
    {
        _onToLongCallback = onToLongCallback;
        _onToSmallCallback = onToSmallCallback;
        _minLength = minLength;
        _maxLength = maxLength;
    }

    public bool Validate(string value)
    {
        if (value.Length < _minLength)
        {
            _onToSmallCallback(value, _minLength);
            return false;
        }
        if (value.Length > _maxLength)
        {
            _onToLongCallback(value, _maxLength);
            return false;
        }
        
        return true;
    }
}