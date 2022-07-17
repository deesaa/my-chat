namespace ChatServer;

public static class MessageRoutsFactory
{
    private static List<IMessageRout> _defaultMessageRouts = new()
    {
        new DefaultMessageRout(),
        new EnterNamePasswordRout(),
        new SendMessageHistoryRout(),
        new SendUsersDataRout(),
        new SetUserTextColorRout(),
    };

    public static List<IMessageRout> GetDefaultMessageRouts()
    {
        return _defaultMessageRouts;
    }
}