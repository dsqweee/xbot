using XBOT.Services.Configuration;

namespace XBOT.Services
{
    public class Leveling
    {
        public static async Task SetPointAsync(SocketMessage Message)
        {
            using (var db = new db())
            {
                var userDiscord = Message.Author as SocketGuildUser;
                var userDb = await db.GetUser(userDiscord.Id);

                userDb.messageCounterForDaily += 1;
                userDb.LastMessageTime = DateTime.Now;

                var Roles = db.Roles_Level.OrderBy(x => x.Level);
                var thisRole = Roles.LastOrDefault(x => x.Level <= userDb.Level);

                var nextLevel = (ulong)Math.Sqrt((userDb.XP + User.ExpPlus) / User.PointFactor);
                if (nextLevel > userDb.Level)
                {
                    var nextRole = Roles.FirstOrDefault(x => x.Level == nextLevel);
                    if (nextRole != null)
                    {
                        if (thisRole != null)
                            await userDiscord.RemoveRoleAsync(thisRole.RoleId);

                        await userDiscord.AddRoleAsync(nextRole.RoleId);
                    }


                    uint result = (uint)(User.DefaultMoney + ((User.DefaultMoney * 0.055) * (userDb.Level + 1)));
                    userDb.money += result;


                    long count = (userDb.Level + 1) * (uint)User.PointFactor * (userDb.Level + 1);
                    long countNext = (userDb.Level + 2) * (uint)User.PointFactor * (userDb.Level + 2);

                    long MessageFnextlevel = (countNext - count) / User.ExpPlus;

                    

                    var Fields = new EmbedBuilder().WithAuthor($"{userDiscord.Username} поднял уровень!", userDiscord.GetAvatarUrl())
                                                   .WithColor(BotSettings.DiscordColor)
                                                   .AddField("Уровень:", $"{userDb.Level + 1}", true)
                                                   .AddField("Опыт:", $"{userDb.XP + User.ExpPlus}", true)
                                                   .AddField("Coins:", $"+{result}", true)
                                                   .WithFooter($"Спасибо за активность в чае <3");
                    await Message.Channel.SendMessageAsync("", false, Fields.Build());
                }
                else if (thisRole != null)
                    await userDiscord.AddRoleAsync(thisRole.RoleId);

                userDb.XP += User.ExpPlus;
                await db.SaveChangesAsync();
            }
        }



    }
}
