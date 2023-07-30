using Discord.Rest;
using XBOT.DataBase.Models.Invites;

namespace XBOT.Services
{
    public class Invite_Service
    {
        public static async Task InviteScanning(IReadOnlyCollection<RestInviteMetadata> Invites)
        {
            using (var db = new db())
            {
                foreach (var Invite in Invites)
                {
                    var InviteDB = db.DiscordInvite.FirstOrDefault(x => x.InviteKey == Invite.Code);
                    if (InviteDB == null)
                    {
                        var User = await db.GetUser(Invite.Inviter.Id);
                        var NewInvite = new DiscordInvite() { AuthorId = User.Id, InviteKey = Invite.Code, DiscordUsesCount = (int)Invite.Uses };
                        db.DiscordInvite.Add(NewInvite);
                    }
                    else if (InviteDB.DiscordUsesCount != Invite.Uses)
                    {
                        InviteDB.DiscordUsesCount = (int)Invite.Uses;
                        db.DiscordInvite.Update(InviteDB);
                    }
                    await db.SaveChangesAsync();
                }
            } 
        }
        //public static async Task InviteDelete(SocketGuildChannel Guild, string InviteId)
        //{
        //    using (db _db = new())
        //    {
        //        var ThisInvite = _db.DiscordInvites.FirstOrDefault(x => x.Key == InviteId);
        //        if (ThisInvite != null)
        //        {
        //            _db.DiscordInvites.Remove(ThisInvite);
        //            await _db.SaveChangesAsync();
        //        }
        //    }
        //}
        public static async Task InviteCreate(SocketInvite Invite)
        {
            using (db _db = new())
            {
                var user = await _db.GetUser(Invite.Inviter.Id);
                var NewInvite = new DiscordInvite() { AuthorId = Invite.Inviter.Id, InviteKey = Invite.Code, DiscordUsesCount = Invite.Uses };
                _db.DiscordInvite.Add(NewInvite);
                await _db.SaveChangesAsync();
            }
        }
        public static async Task<RestInviteMetadata> JoinedUserInviteAttach(IReadOnlyCollection<RestInviteMetadata> Invites,SocketGuildUser JoinedUser)
        {
            using (var db = new db())
            {
                RestInviteMetadata ReadedInvite = null;
                foreach (var InviteDs in Invites)
                {
                    var InviteDb = db.DiscordInvite.FirstOrDefault(x => x.InviteKey == InviteDs.Code);
                    if (InviteDb != null && InviteDb.DiscordUsesCount != InviteDs.Uses)
                    {
                        var JoinedUserDb = await db.GetUser(JoinedUser.Id);
                        if(JoinedUserDb.RefferalInvite == null)
                        {
                            var NewRef = new DiscordInvite_ReferralLink { CreationTime = DateTime.Now,InviteId = InviteDb.Id,UserId = JoinedUserDb.Id};
                            db.ReferralLinks.Add(NewRef);
                        }

                        var NewConnectionAudit = new DiscordInvite_ConnectionAudit {ConnectionTime = DateTime.Now,InviteId = InviteDb.Id,UserId = JoinedUserDb.Id };
                        db.ConnectionAudits.Add(NewConnectionAudit);

                        InviteDb.DiscordUsesCount++;
                        db.DiscordInvite.Update(InviteDb);
                        await db.SaveChangesAsync();
                        ReadedInvite = InviteDs;
                        break;
                    }
                }
                return ReadedInvite;
            }
        }
    }
}
