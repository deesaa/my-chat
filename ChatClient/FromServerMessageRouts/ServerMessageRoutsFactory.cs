namespace ChatClient.FromServerMessageRouts;

public static class ServerMessageRoutsFactory
{
    private static List<IServerMessageRout> _defaultServerMessageRouts = new()
    {
        new DefaultMessageRout(),
        new LoginSuccessRout(),
        new WrongNameOrPassRout(),
        new UserJoinedRout(),
        new UserLeftRout(),
        new UserChangedTextColorRout(),
        new UsersOnServerDataRout()

    };

    public static List<IServerMessageRout> GetDefaultServerMessageRouts() => _defaultServerMessageRouts;
}