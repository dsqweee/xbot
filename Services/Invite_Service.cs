using Discord.Rest;
using XBOT.DataBase.Models.Invites;

namespace XBOT.Services;

public class Invite_Service
{
    private readonly DiscordSocketClient _client;
    private readonly Db _db;

    public Invite_Service(DiscordSocketClient client, Db db)
    {
        _client = client;
        _db = db;
    }

    public async Task InviteScanning()
    {
        var Invites = await _client.Guilds.First().GetInvitesAsync();
        foreach (var Invite in Invites)
        {
            var InviteDB = _db.DiscordInvite.FirstOrDefault(x => x.InviteKey == Invite.Code);
            if (InviteDB == null)
            {
                var User = await _db.GetUser(Invite.Inviter.Id);
                var NewInvite = new DiscordInvite() { AuthorId = User.Id, InviteKey = Invite.Code, DiscordUsesCount = (int)Invite.Uses };
                _db.DiscordInvite.Add(NewInvite);
            }
            else if (InviteDB.DiscordUsesCount != Invite.Uses)
            {
                InviteDB.DiscordUsesCount = (int)Invite.Uses;
                _db.DiscordInvite.Update(InviteDB);
            }
            await _db.SaveChangesAsync();
        }
    }
    //public static async Task InviteDelete(SocketGuildChannel Guild, string InviteId)
    //{
    //    using (Db _db = new ())
    //    {
    //        var ThisInvite = db.DiscordInvites.FirstOrDefault(x => x.Key == InviteId);
    //        if (ThisInvite != null)
    //        {
    //            db.DiscordInvites.Remove(ThisInvite);
    //            await db.SaveChangesAsync();
    //        }
    //    }
    //}
    public async Task InviteCreate(SocketInvite Invite)
    {
        var user = await _db.GetUser(Invite.Inviter.Id);
        var NewInvite = new DiscordInvite() { AuthorId = Invite.Inviter.Id, InviteKey = Invite.Code, DiscordUsesCount = Invite.Uses };
        _db.DiscordInvite.Add(NewInvite);
        await _db.SaveChangesAsync();
    }
    public async Task<RestInviteMetadata> JoinedUserInviteAttach(SocketGuildUser JoinedUser)
    {
        RestInviteMetadata ReadedInvite = null;
        var Invites = await _client.Guilds.First().GetInvitesAsync();
        foreach (var InviteDs in Invites)
        {
            var InviteDb = _db.DiscordInvite.FirstOrDefault(x => x.InviteKey == InviteDs.Code);
            if (InviteDb != null && InviteDb.DiscordUsesCount != InviteDs.Uses)
            {
                var JoinedUserDb = await _db.GetUser(JoinedUser.Id);
                if (JoinedUserDb.RefferalInvite == null)
                {
                    var NewRef = new DiscordInvite_ReferralLink { CreationTime = DateTime.Now, InviteId = InviteDb.Id, UserId = JoinedUserDb.Id };
                    _db.ReferralLinks.Add(NewRef);
                }

                var NewConnectionAudit = new DiscordInvite_ConnectionAudit { ConnectionTime = DateTime.Now, InviteId = InviteDb.Id, UserId = JoinedUserDb.Id };
                _db.ConnectionAudits.Add(NewConnectionAudit);

                InviteDb.DiscordUsesCount++;
                _db.DiscordInvite.Update(InviteDb);
                await _db.SaveChangesAsync();
                ReadedInvite = InviteDs;
                break;
            }
        }
        return ReadedInvite;
    }
}
