using Microsoft.EntityFrameworkCore;
using XBOT.DataBase.Models.Invites;

namespace XBOT.Services;

public class Refferal_Service
{
    //private readonly Db _db;

    //public Refferal_Service(Db db)
    //{
    //    _db = db;
    //}

    public async Task ReferalRoleScaningUser(IReadOnlyCollection<SocketGuildUser> Users)
    {
        using var _db = new Db();
        foreach (var currentUser in Users)
        {
            var userDb = await _db.GetUser(currentUser.Id);
            var RefferalRole = await AuthorReferalRole(userDb.Id);
            if (RefferalRole == null)
                break;
            foreach (var RefRole in _db.ReferralRole)
            {
                if (currentUser.Roles.Any(x => x.Id == RefRole.RoleId) && RefRole.Id != RefferalRole.Id)
                    await currentUser.RemoveRoleAsync(RefRole.RoleId);
            }

            await currentUser.AddRoleAsync(RefferalRole.RoleId);
        }
    }

    public async Task<(int CountRef, int WriteInWeek, int Level5up)> GetRefferalValue(User userDb)
    {
        using var _db = new Db();
        int CountRef = 0;
        int UserWriteInWeek = 0;
        int UserLevel5Up = 0;
        foreach (var MyInvite in userDb.MyInvites)
        {
            foreach (var Ref in MyInvite.ReferralLinks)
            {
                CountRef++;
                var user = await _db.GetUser(Ref.UserId);
                if (user.LastMessageTime > DateTime.Now.AddDays(-7))
                    UserWriteInWeek++;

                if (user.Level >= 5)
                    UserLevel5Up++;

            }
        }
        return (CountRef, UserWriteInWeek, UserLevel5Up);
    }

    public async Task<DiscordInvite_ReferralRole> AuthorReferalRole(ulong AuthorId)
    {
        using var _db = new Db();
        var userDb = await _db.User.Include(x => x.MyInvites).ThenInclude(x => x.ReferralLinks).FirstOrDefaultAsync(x => x.Id == AuthorId);
        var UserValue = await GetRefferalValue(userDb);

        var Role = _db.ReferralRole.FirstOrDefault(x => UserValue.CountRef >= x.UserJoinedValue && UserValue.WriteInWeek >= x.UserWriteInWeekValue && UserValue.Level5up >= x.UserUp5LevelValue);
        return Role;
    }
}
