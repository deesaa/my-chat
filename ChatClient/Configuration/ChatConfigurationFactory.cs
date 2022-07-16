namespace ChatClient.Configuration;

public static class ChatConfigurationFactory
{
    public static ChatConfiguration GetDefaultConfiguration()
    {
        ChatConfiguration config = new ChatConfiguration();
        
        config.SetValidator("name", ValidatorFactory.GetDefaultNameValidator());
        config.SetSterilizer("name", new EmptyCharsSterilizer());
        
        config.SetValidator("password", ValidatorFactory.GetDefaultPasswordValidator());
        config.SetSterilizer("password", new EmptyCharsSterilizer());
        
        config.SetValidator("message", ValidatorFactory.GetDefaultMessageValidator());
        config.SetSterilizer("message", new TrimSterilizer());
        
        return config;
    }
}