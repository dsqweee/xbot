using Microsoft.EntityFrameworkCore;
using XBOT.DataBase.Models.Invites;

namespace XBOT.Services
{
    public class Refferal_Service
    {
        public static async Task ReferalRoleScaningUser(IReadOnlyCollection<SocketGuildUser> Users)
        {
            using (var _db = new db())
            {
                foreach (var currentUser in Users)
                {
                    var userDb = _db.User.FirstOrDefault(x=>x.Id == currentUser.Id);
                    var RefferalRole = AuthorReferalRole(userDb.Id);
                    foreach (var RefRole in _db.ReferralRole)
                    {
                        if (currentUser.Roles.Any(x => x.Id == RefRole.Id) && RefRole.Id != RefferalRole.Id)
                            await currentUser.RemoveRoleAsync(RefRole.Id);
                    }

                    await currentUser.AddRoleAsync(RefferalRole.Id);
                }
            }
        }

        public static DiscordInvite_ReferralRole AuthorReferalRole(ulong AuthorId)
        {
            using (var _db = new db())
            {
                var userDb = _db.User.Include(x => x.MyInvites).ThenInclude(x => x.ReferralLinks).ThenInclude(x => x.User).FirstOrDefault(x => x.Id == AuthorId);
                int CountRef = 0;
                int UserWriteInWeek = 0;
                int UserLevel5Up = 0;
                foreach (var MyInvite in userDb.MyInvites)
                {
                    foreach (var Ref in MyInvite.ReferralLinks)
                    {
                        CountRef++;
                        if (Ref.User.LastMessageTime > DateTime.Now.AddDays(-7))
                            UserWriteInWeek++;

                        if (Ref.User.Level >= 5)
                            UserLevel5Up++;

                    }
                }

                var Role = _db.ReferralRole.FirstOrDefault(x => CountRef >= x.UserJoinedValue && UserWriteInWeek >= x.UserWriteInWeekValue && UserLevel5Up >= x.UserUp5LevelValue);

                return Role;
            }
        }
    }
}
