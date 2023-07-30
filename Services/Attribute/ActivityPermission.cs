namespace XBOT.Services.Attribute
{
    sealed class ActivityPermission : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            using (db _db = new())
            {
                string Reason = "";
                var UserGetter = await _db.GetUser(context.User.Id);
                User UserSetter = null;
                var UserDiscord = await context.Guild.GetUserAsync(context.Message.MentionedUserIds.First());


                if (!UserDiscord.IsBot)
                {
                    UserSetter = await _db.GetUser(UserDiscord.Id);
                }

                if (command.Name == "transfer")
                {
                    bool isUserGetterActive = CheckUserActivity(UserGetter);
                    bool isUserSetterActive = CheckUserActivity(UserSetter);

                    bool CheckUserActivity(User user)
                    {
                        return user.Level >= 5 && (user.messageCounterForDaily >= 30 || (user.daily_Time - DateTime.Now).TotalHours <= 8);
                    }

                    if (!isUserGetterActive)
                        Reason = "Ваш уровень активности не соответствует требованиям для перевода денег.";
                    else if (!isUserSetterActive)
                        Reason = $"Уровень активности <@{UserSetter}> не соответствует требованиям для перевода денег.";
                }
                else if (command.Name == "reputation")
                {
                    bool isUserGetterActive = CheckUserActivity(UserGetter);
                    bool isUserSetterActive = CheckUserActivity(UserSetter);

                    bool CheckUserActivity(User user)
                    {
                        return user.Level >= 5 && (user.messageCounterForDaily >= 30 || (user.daily_Time - DateTime.Now).TotalHours <= 8) && user.streak < 5;
                    }
                    if (!isUserGetterActive)
                        Reason = "Ваш уровень активности не соответствует требованиям для выдачи репутации.";
                    else if (!isUserSetterActive)
                        Reason = $"Уровень активности <@{UserSetter}> не соответствует требованиям для выдачи репутации.";
                }




                if (Reason == null)
                    return await Task.FromResult(PreconditionResult.FromSuccess());

                return await Task.FromResult(PreconditionResult.FromError(Reason));
            }
        }
    }
}
