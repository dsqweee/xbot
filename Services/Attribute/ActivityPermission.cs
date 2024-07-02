using Microsoft.Extensions.DependencyInjection;
using XBOT.Services.Configuration;

namespace XBOT.Services.Attribute;

sealed class ActivityPermission : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        ulong userId = context.User.Id;
        //if (userId == BotSettings.xId)
        //    return PreconditionResult.FromSuccess();

        var _db = services.GetRequiredService<Db>();
        //using var _db = new Db();
        var userGetter = await _db.GetUser(userId);
        var userSetter = await GetUserSetter(context, _db);


        bool isUserGetterActive = CheckUserActivity(userGetter);
        bool isUserSetterActive = userSetter is null ? true : CheckUserActivity(userSetter);

        bool CheckUserActivity(User user)
        {
            bool isActive = user.Level >= 5;
            if (user.messageCounterForDaily >= 30 || (user.daily_Time - DateTime.Now).TotalHours <= 8)
            {
                if (command.Name == "reputation" || command.Name == "rep")
                {
                    isActive = user.streak >= 5;
                }
            }
            return isActive;
        }

        string reason = "";
        if (!isUserGetterActive)
            reason = "Ваш уровень активности не соответствует требованиям.";
        else if (!isUserSetterActive)
            reason = $"Уровень активности <@{userSetter.Id}> не соответствует требованиям.";

        if (reason == "")
            return PreconditionResult.FromSuccess();

        reason += "\nТребования:\n • 5 уровень\n • Daily Streak 5\n • Просто общаться в чате :)";

        return PreconditionResult.FromError(reason);











        //string Reason = "";
        //var UserGetter = await db.GetUser(context.User.Id);
        //User UserSetter = null;

        //if (UserGetter.Id == BotSettings.xId)
        //    return await Task.FromResult(PreconditionResult.FromSuccess());

        //var mentionUser = context.Message.MentionedUserIds.FirstOrDefault();
        //if (mentionUser is not 0)
        //{
        //    var UserDiscord = await context.Guild.GetUserAsync(mentionUser);
        //    if (UserDiscord.IsBot)
        //        UserSetter = await db.GetUser(UserDiscord.Id);
        //}

        //if (command.Name == "transfer")
        //{
        //    bool isUserGetterActive = CheckUserActivity(UserGetter);
        //    bool isUserSetterActive = CheckUserActivity(UserSetter);

        //    bool CheckUserActivity(User user)
        //    {
        //        return user.Level >= 5 && (user.messageCounterForDaily >= 30 || (user.daily_Time - DateTime.Now).TotalHours <= 8);
        //    }

        //    if (!isUserGetterActive)
        //        Reason = "Ваш уровень активности не соответствует требованиям для перевода денег.";
        //    else if (!isUserSetterActive)
        //        Reason = $"Уровень активности <@{UserSetter}> не соответствует требованиям для перевода денег.";
        //}
        //else if (command.Name == "reputation")
        //{
        //    bool isUserGetterActive = CheckUserActivity(UserGetter);
        //    bool isUserSetterActive = CheckUserActivity(UserSetter);

        //    bool CheckUserActivity(User user)
        //    {
        //        return user.Level >= 5 && (user.messageCounterForDaily >= 30 || (user.daily_Time - DateTime.Now).TotalHours <= 8) && user.streak < 5;
        //    }
        //    if (!isUserGetterActive)
        //        Reason = "Ваш уровень активности не соответствует требованиям для выдачи репутации.";
        //    else if (!isUserSetterActive)
        //        Reason = $"Уровень активности <@{UserSetter}> не соответствует требованиям для выдачи репутации.";
        //}

        //if (Reason == null)
        //    return await Task.FromResult(PreconditionResult.FromSuccess());

        //return await Task.FromResult(PreconditionResult.FromError(Reason));

    }

    private async Task<User> GetUserSetter(ICommandContext context, Db db)
    {
        var mentionUser = context.Message.MentionedUserIds.FirstOrDefault();
        if (mentionUser != 0)
        {
            var userDiscord = await context.Guild.GetUserAsync(mentionUser);
            if (!userDiscord.IsBot)
            {
                return await db.GetUser(userDiscord.Id);
            }
        }
        return null;
    }
}
