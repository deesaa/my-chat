public static class ValidatorFactory
{
    public static IValidator GetDefaultNameValidator()
    {
        return new LengthValidator(4, 16,
            (name, maxLength) =>
                Console.WriteLine($"Name {name} is too long. Max size is {maxLength} chars"),
            (name, minLength) =>
                Console.WriteLine($"Name {name} is too small. Min size is {minLength} chars"));
    }
    
    public static IValidator GetDefaultMessageValidator()
    {
        return new LengthValidator(1, 128,
            (message, maxLength) => 
                Console.WriteLine($"Message {message} is too long. Max size is {maxLength} chars"),
            (message, minLength) =>
                Console.WriteLine($"Message {message} is too small. Min size is {minLength} chars"));
    }
}