﻿using static XBOT.DataBase.Guild_Logs;
using XBOT.Services.DiscordAudit.Data;
using XBOT.Services.DiscordAudit;
using XBOT.Services.Configuration;
using System.Threading.Channels;

namespace XBOT.Services;

public class Guild_Logs_Service
{
    private readonly Invite_Service _invite;
    private readonly Db _db;

    public Guild_Logs_Service(Invite_Service invite, Db db)
    {
        _invite = invite;
        _db = db;
    }
    public async Task InVoicelogs(SocketGuildUser User, SocketVoiceState ActionBefore, SocketVoiceState ActionAfter)
    {
        //using var _db = new Db();
        var Guild_Log = _db.Guild_Logs.FirstOrDefault(x => x.Type == ChannelsTypeEnum.VoiceAction);
        if (Guild_Log == null)
            return;

        var chnl = User.Guild.GetTextChannel(Guild_Log.TextChannelId);
        if (chnl == null)
            return;


        var emb = new EmbedBuilder()
            .WithColor(BotSettings.DiscordColor)
            .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
            .WithFooter(x => x.WithText($"id: {User.Id}"))
            .AddField($"Пользователь", $"{User.Mention}", true);

        if (ActionBefore.VoiceChannel != null)
        {
            if (ActionAfter.VoiceChannel == null)
            {
                emb.WithAuthor(" - Выход из Голосового чата", User.GetAvatarUrl())
                   .AddField($"Выход из", $"{ActionBefore.VoiceChannel.Mention}", true);
            }
            else if (ActionBefore.VoiceChannel != ActionAfter.VoiceChannel)
            {
                emb.WithAuthor(" - Переход в другой Голосовой канал", User.GetAvatarUrl())
                   .AddField($"Переход из", $"{ActionBefore.VoiceChannel.Mention}", true)
                   .AddField($"Переход в", $"{ActionAfter.VoiceChannel.Mention}", true);
            }
            else
            {
                var Audit = new AuditsUserAction();
                string text = string.Empty;

                emb.AddField($"В канале", $"{ActionBefore.VoiceChannel.Mention}", true);
                var TypeAction = VoiceAuditActionEnum.Defect;

                if (ActionAfter.IsDeafened)
                {
                    TypeAction = VoiceAuditActionEnum.AdminDeafened;
                    text = "Администратор отключил звук";
                }
                else if (ActionAfter.IsMuted)
                {
                    TypeAction = VoiceAuditActionEnum.AdminMute;
                    text = "Администратор отключил микрофон";
                }
                else if (ActionBefore.IsDeafened)
                {
                    TypeAction = VoiceAuditActionEnum.AdminUnDeafened;
                    text = "Администратор включил звук";
                }
                else if (ActionBefore.IsMuted)
                {
                    TypeAction = VoiceAuditActionEnum.AdminUnMute;
                    text = "Администратор включил микрофон";
                }
                else if (ActionAfter.IsSelfDeafened)
                    text = "Пользователь отключил звук";
                else if (ActionAfter.IsSelfMuted)
                    text = "Пользователь отключил микрофон";
                else if (ActionAfter.IsStreaming)
                    text = "Пользователь запустил стрим";
                else if (ActionBefore.IsSelfDeafened)
                    text = "Пользователь включил звук";
                else if (ActionBefore.IsSelfMuted)
                    text = "Пользователь включил микрофон";
                else if (ActionBefore.IsStreaming)
                    text = "Пользователь закончил стрим";
                else if (ActionBefore.IsVideoing)
                    text = "Пользователь выключил камеру";
                else if (ActionAfter.IsVideoing)
                    text = "Пользователь включил камеру";
                else if (ActionBefore.IsSuppressed)
                    text = $"Пользователь выключил голос для записи";
                else if (ActionAfter.IsSuppressed)
                    text = "Пользователь включил голос для записи";
                

                if (TypeAction != VoiceAuditActionEnum.Defect)
                    Audit = User.AdminVoiceAudit(User.Id, 1, TypeAction).Result?.FirstOrDefault();

                if (Audit != null && Audit.User != null)
                    emb.AddField($"Администратор", $"{Audit.User.Mention}");

                emb.WithAuthor(" - " + text, User.GetAvatarUrl());
            }
            // Переход из одного чата в другой

        }

        if (ActionAfter.VoiceChannel != null && ActionBefore.VoiceChannel == null)
        {
            emb.WithAuthor(" - Вход в голосовой чат", User.GetAvatarUrl())
               .AddField($"Вход в ", $"{ActionAfter.VoiceChannel.Mention}", true);
        }

        await chnl.SendMessageAsync("", false, emb.Build());

    }

    public async Task InJoinedUser(SocketGuildUser user)
    {
        //using var _db = new Db();
        var InUserJoinedInvite = await _invite.JoinedUserInviteAttach(user);

        var Guild_Log = _db.Guild_Logs.FirstOrDefault(x => x.Type == ChannelsTypeEnum.Join);
        if (Guild_Log == null)
            return;

        var chnl = user.Guild.GetTextChannel(Guild_Log.TextChannelId);
        if (chnl == null)
            return;

        var builder = new EmbedBuilder().WithColor(BotSettings.DiscordColor);

        if (InUserJoinedInvite != null)
            builder.AddField("Приглашение", $"Code: {InUserJoinedInvite.Code}\nСоздатель: {InUserJoinedInvite.Inviter.Mention}");

        builder.WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
               .WithAuthor($"Пользователь присоединился: {user.Username}", user.GetAvatarUrl())
               .WithDescription($"Имя: {user.Mention}\n" +
                                $"Участников: {user.Guild.MemberCount}\n" +
                                $"Аккаунт создан: {user.CreatedAt.ToUniversalTime():dd.MM.yyyy HH:mm:ss}");
        await chnl.SendMessageAsync("", false, builder.Build());

    }

    public async Task InLeftedAndKickedUser(SocketGuild Guild, SocketUser user)
    {
        var builder = new EmbedBuilder().WithColor(BotSettings.DiscordColor)
                                        .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                        .AddField($"Пользователь", user.Mention, true)
                                        .AddField($"Участников", Guild.MemberCount)
                                        .AddField($"Аккаунт Создан", user.CreatedAt.ToString("dd.MM.yyyy HH:mm:ss"), true)
                                        .AddField($"Был на сервере с", (user as SocketGuildUser).JoinedAt.Value.ToString("dd.MM.yyyy HH:mm:ss"), true);

        SocketTextChannel GetLogChannel(bool left)
        {
            //using var _db = new Db();
            var LogChannel = _db.Guild_Logs.FirstOrDefault(x => x.Type == (left ? ChannelsTypeEnum.Left : ChannelsTypeEnum.Kick));

            var LogDiscord = Guild.GetTextChannel(LogChannel.TextChannelId);
            return LogDiscord;
        }

        var LeftChannel = GetLogChannel(true);

        if (LeftChannel != null)
        {
            builder.WithAuthor($"{user} - Пользователь вышел", user.GetAvatarUrl());
            await LeftChannel.SendMessageAsync("", false, builder.Build());
        }
            


        var KickChannel = GetLogChannel(false);
        if (KickChannel != null)
        {
            List<Audits> Kicks = await Guild.KickAudit(user.Id, 1);
            var Kicked = Kicks?.FirstOrDefault();
            bool UserKicked = false;
            if (Kicked != null && (DateTime.Now - Kicked.Time).TotalSeconds < 1.5)
                UserKicked = true;

            if (UserKicked)
            {
                builder.WithAuthor($"{user} - Пользователь кикнут", user.GetAvatarUrl())
                       .AddField($"Кикнул", Kicked.User.Mention, true)
                       .AddField($"Причина", $"{(string.IsNullOrWhiteSpace(Kicked.Reason) ? "-" : Kicked.Reason)}");

                await KickChannel?.SendMessageAsync("", false, builder.Build());
            }
        }
    }

    public async Task InUserUnBanned(SocketUser user, SocketGuild guild)
        => await BanOrUnBan(user, guild, ChannelsTypeEnum.UnBan);
    public async Task InUserBanned(SocketUser user, SocketGuild guild)
        => await BanOrUnBan(user, guild, ChannelsTypeEnum.Ban);
    private async Task BanOrUnBan(SocketUser user, SocketGuild Guild, ChannelsTypeEnum ChannelType)
    {
        //using var _db = new Db();
        var Guild_Log = _db.Guild_Logs.FirstOrDefault(x => x.Type == ChannelType);
        if (Guild_Log == null)
            return;

        var ChannelForMessage = Guild.GetTextChannel(Guild_Log.TextChannelId);
        if (ChannelForMessage == null)
            return;


        List<Audits> Bans;
        if (ChannelType == ChannelsTypeEnum.Ban)
            Bans = await Guild.BanAudit(user.Id, 1);
        else
            Bans = await Guild.UnBanAudit(user.Id, 1);

        Audits Banned = Bans.FirstOrDefault();
        var builder = new EmbedBuilder().WithColor(BotSettings.DiscordColor)
                                        .WithTimestamp(DateTimeOffset.Now.ToUniversalTime())
                                        .WithAuthor($"{user} - Пользователь {(ChannelType == ChannelsTypeEnum.Ban ? "Забанен" : "Разбанен")}")
                                        .AddField($"Пользователь", user.Mention, true)
                                        .AddField($"{(ChannelType == ChannelsTypeEnum.Ban ? "Забанил" : "Разбанил")}", Banned.User.Mention, true)
                                        .AddField($"Причина бана", $"{(string.IsNullOrWhiteSpace(Banned.Reason) ? "-" : Banned.Reason)}");
        await ChannelForMessage.SendMessageAsync("", false, builder.Build());

    }

    public async Task DeleteMessage(Cacheable<IMessage, ulong> CachedMessage, Cacheable<IMessageChannel, ulong> arg2)
        => await DeletedAndEditedMessage(CachedMessage, null);

    public async Task EditedMessage(Cacheable<IMessage, ulong> CachedMessage, SocketMessage MessageNow, ISocketMessageChannel Channel)
        => await DeletedAndEditedMessage(CachedMessage, MessageNow);

    public async Task DeletedAndEditedMessage(Cacheable<IMessage, ulong> CachedMessage, SocketMessage MessageNow)
    {
        //using var _db = new Db();
        var Message = await CachedMessage.GetOrDownloadAsync();
        Console.WriteLine($"Message - {Message}");
        Console.WriteLine($"Message.Author - {Message.Author}");
        Console.WriteLine($"Message.Content - {Message.Content}");
        Console.WriteLine($"MessageNow.Content - {MessageNow?.Content}");
        if (Message == null || Message.Author == null || Message.Author.IsBot || Message?.Content.Length > 1023 || MessageNow?.Content.Length > 1023)
            return;

        if (Message.Author is not SocketGuildUser User)
            return;

        var GuildLog_Type = MessageNow == null ? ChannelsTypeEnum.MessageDelete : ChannelsTypeEnum.MessageEdit;
        var Guild_Log = _db.Guild_Logs.FirstOrDefault(x => x.Type == GuildLog_Type);
        if (Guild_Log == null)
            return;

        var MessageChannel = User.Guild.GetTextChannel(Guild_Log.TextChannelId);
        if (MessageChannel == null)
            return;

        var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor);


        if (MessageNow == null)
            emb.WithAuthor($"Сообщение удалено", Message.Author.GetAvatarUrl())
               .AddField("Сообщение", string.IsNullOrWhiteSpace(Message.Content) ? "-" : Message.Content);
        else
        {
            if (MessageNow.Attachments.Count > 0)
                emb.WithImageUrl(MessageNow.Attachments.FirstOrDefault().Url);

            emb.WithAuthor($"Сообщение изменено", Message.Author.GetAvatarUrl())
               .AddField("Прошлое", string.IsNullOrWhiteSpace(Message.Content) ? "-" : Message.Content)
               .AddField("Новое", string.IsNullOrWhiteSpace(MessageNow.Content) ? "-" : MessageNow.Content);
        }
            

        emb.AddField("Канал", $"<#{Message.Channel.Id}>", true)
           .AddField("Отправитель", Message.Author.Mention, true)
           .WithTimestamp(DateTime.Now.ToUniversalTime());

        await MessageChannel.SendMessageAsync("", false, emb.Build());
    }

    public async Task Birthday(SocketGuildUser user)
    {
        //using var _db = new Db();
        var emb = new EmbedBuilder()
            .WithColor(BotSettings.DiscordColor)
            .WithAuthor("С днем рождения солнышко🎉");

        var GuildLogs = _db.Guild_Logs.FirstOrDefault(x => x.Type == ChannelsTypeEnum.BirthDay);
        if (GuildLogs == null)
            return;

        var Channel = user.Guild.GetTextChannel(GuildLogs.Id);
        if (Channel == null)
            return;

        var prefix = _db.Settings.FirstOrDefault().Prefix;

        var userDb = _db.User.FirstOrDefault(x => x.Id == user.Id);
        bool daynow = false;
        if (userDb.BirthDate.Month == DateTime.Now.Month && userDb.BirthDate.Day == DateTime.Now.Day)
            daynow = true;

        emb.WithDescription($"У пользователя 🎊{user.Mention}🎊 {(daynow ? "сегодня" : "недавно")} день рождения\nдавайте вместе поздравим его🎊")
           .WithFooter($"Укажи день рождения чтобы получать уведомления - {prefix}birthdayset");
        await Channel.SendMessageAsync(user.Mention, false, emb.Build());
    }
}
