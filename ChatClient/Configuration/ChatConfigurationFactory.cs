namespace ChatClient.Configuration;

public static class ChatConfigurationFactory
{
    public static ChatConfiguration GetDefaultConfiguration()
    {
        ChatConfiguration config = new ChatConfiguration();
        
        config.SetValidator("name", ValidatorFactory.GetDefaultNameValidator());
        config.SetSanitizer("name", new EmptyCharsSanitizer());
        
        config.SetValidator("password", ValidatorFactory.GetDefaultPasswordValidator());
        config.SetSanitizer("password", new EmptyCharsSanitizer());
        
        config.SetValidator("message", ValidatorFactory.GetDefaultMessageValidator());
        config.SetSanitizer("message", new TrimSanitizer());
        
        return config;
    }
}