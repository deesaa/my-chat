namespace ChatClient.Configuration;

public static class ChatConfigurationFactory
{
    public static ChatConfiguration GetDefaultConfiguration()
    {
        ChatConfiguration config = new ChatConfiguration();
        
        config.SetValidator("username", ValidatorFactory.GetDefaultNameValidator());
        config.SetSterilizer("username", new EmptyCharsSterilizer());
        
        config.SetValidator("password", ValidatorFactory.GetDefaultPasswordValidator());
        config.SetSterilizer("password", new EmptyCharsSterilizer());
        
        config.SetValidator("message", ValidatorFactory.GetDefaultMessageValidator());
        config.SetSterilizer("message", new TrimSterilizer());
        
        return config;
    }
}

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
    
    public static IValidator GetDefaultPasswordValidator()
    {
        return new LengthValidator(4, 32,
            (name, maxLength) =>
                Console.WriteLine($"Password {name} is too long. Max size is {maxLength} chars"),
            (name, minLength) =>
                Console.WriteLine($"Password {name} is too small. Min size is {minLength} chars"));
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