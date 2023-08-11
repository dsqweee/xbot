using Discord.Rest;

namespace XBOT.Services.DiscordAudit.Data;

public static class RunningAudit
{
    public static async Task<List<Audits>> Running(this SocketGuild Guild, ulong TargetId, int Count, ActionType Type)
    {
        var Logs = Guild.GetAuditLogsAsync(Count, null, null, null, Type);
        List<Audits> Audit = new List<Audits>();

        await foreach (var LogRead in Logs)
        {
            foreach (var Log in LogRead)
            {
                var Target = Type == ActionType.Kick ? (Log.Data as KickAuditLogData).Target :
                             Type == ActionType.Ban ? (Log.Data as BanAuditLogData).Target :
                             Type == ActionType.Unban ? (Log.Data as UnbanAuditLogData).Target : null;
                if (Target.Id == TargetId)
                    Audit.Add(new Audits() { Id = Log.Id, Reason = Log.Reason, Target = Target, Time = Log.CreatedAt, User = Log.User });
            }
        }

        return Audit.OrderByDescending(x => x.Time).ToList();
    }

    public static async Task<List<AuditsUserAction>> RunningVoiceAction(this SocketGuildUser user, ulong TargetId, int Count, VoiceAuditActionEnum Type)
    {
        var Logs = user.Guild.GetAuditLogsAsync(Count, null, null, null, ActionType.MemberUpdated);
        List<AuditsUserAction> Audit = new();

        await foreach (var LogRead in Logs)
        {
            foreach (var Log in LogRead)
            {
                var Target = Log.Data as MemberUpdateAuditLogData;
                switch (Type)
                {
                    case VoiceAuditActionEnum.AdminMute:
                        if (!((bool)!Target.Before.Mute && (bool)Target.After.Mute))
                            Type = VoiceAuditActionEnum.Defect;
                        break;
                    case VoiceAuditActionEnum.AdminUnMute:
                        if (!((bool)Target.Before.Mute && (bool)!Target.After.Mute))
                            Type = VoiceAuditActionEnum.Defect;
                        break;
                    case VoiceAuditActionEnum.AdminDeafened:
                        if (!((bool)!Target.Before.Deaf && (bool)Target.After.Deaf))
                            Type = VoiceAuditActionEnum.Defect;
                        break;
                    case VoiceAuditActionEnum.AdminUnDeafened:
                        if (!((bool)Target.Before.Deaf && (bool)!Target.After.Deaf))
                            Type = VoiceAuditActionEnum.Defect;
                        break;
                }

                if (Target.Target.Id == TargetId && Type != VoiceAuditActionEnum.Defect)
                    Audit.Add(new AuditsUserAction() { Id = Log.Id, Reason = Log.Reason, Target = Target.Target, Time = Log.CreatedAt, User = Log.User, AfterBeforeInfo = Target.After, TargetBeforeInfo = Target.Before });
            }
        }
        return Audit.OrderByDescending(x => x.Time).ToList();
    }
}
