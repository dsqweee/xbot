using Discord.Rest;

namespace XBOT.Services.DiscordAudit.Data;

public class Audits
{
    public ulong Id { get; set; }
    public IUser Target { get; set; }
    public IUser User { get; set; }
    public string Reason { get; set; }
    public DateTimeOffset Time { get; set; }
}

public class AuditsUserAction
{
    public ulong Id { get; set; }
    public IUser Target { get; set; }
    public MemberInfo TargetBeforeInfo { get; set; }
    public MemberInfo AfterBeforeInfo { get; set; }
    public IUser User { get; set; }
    public string Reason { get; set; }
    public DateTimeOffset Time { get; set; }
}

public enum VoiceAuditActionEnum : byte
{
    AdminMute,
    AdminUnMute,
    AdminDeafened,
    AdminUnDeafened,
    Defect
}
