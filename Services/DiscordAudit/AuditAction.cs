using XBOT.Services.DiscordAudit.Data;

namespace XBOT.Services.DiscordAudit;

public static class AuditAction
{
    public static Task<List<Audits>> BanAudit(this SocketGuild guild, ulong targetId, int count)
    {
        return RunningAudit.Running(guild, targetId, count, ActionType.Ban);
    }

    public static Task<List<Audits>> UnBanAudit(this SocketGuild guild, ulong targetId, int count)
    {
        return RunningAudit.Running(guild, targetId, count, ActionType.Unban);
    }

    public static Task<List<Audits>> KickAudit(this SocketGuild guild, ulong targetId, int count)
    {
        return RunningAudit.Running(guild, targetId, count, ActionType.Kick);
    }

    public static Task<List<AuditsUserAction>> AdminVoiceAudit(this SocketUser user, ulong targetId, int count, VoiceAuditActionEnum type)
    {
        var GuildUser = user as SocketGuildUser;
        return GuildUser.RunningVoiceAction(targetId, count, type);
    }
}
