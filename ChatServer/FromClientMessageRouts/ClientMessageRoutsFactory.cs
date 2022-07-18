namespace ChatServer;

public static class ClientMessageRoutsFactory
{
    private static List<IClientMessageRout> _defaultMessageRouts = new()
    {
        new DefaultClientMessageRout(),
        new EnterNamePasswordRout(),
        new SendClientMessageHistoryRout(),
        new SendUsersDataRout(),
        new SetUserTextColorRout(),
    };

    public static List<IClientMessageRout> GetDefaultClientMessageRouts() =>_defaultMessageRouts;
}