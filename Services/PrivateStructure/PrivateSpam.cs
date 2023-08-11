namespace XBOT.Services.PrivateStructure;

public class PrivateSpam
{
    private static readonly Dictionary<ulong, DateTime> UserList = new();

    public static bool CheckSpamPrivate(SocketGuildUser User)
    {
        foreach (var item in UserList)
        {
            if((DateTime.Now - item.Value).TotalSeconds > 10)
                UserList.Remove(item.Key);
        }

        if (UserList.Any(x => x.Key == User.Id))
            return true;

        return false;
    }

    public static void AddUser(SocketGuildUser User)
        => UserList.Add(User.Id,DateTime.Now);
}
