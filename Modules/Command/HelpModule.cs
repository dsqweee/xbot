using XBOT.Services.Attribute;
using XBOT.Services.Attribute.CommandList;
using XBOT.Services.Configuration;

namespace XBOT.Modules.Command
{
    [Summary("Команды управления ботом"), Name("Help")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RequireBotPermission(GuildPermission.SendMessages)]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IServiceProvider _provider;
        private readonly Db _db;

        public HelpModule(CommandService service, IServiceProvider provider, Db db)
        {
            _service = service;
            _provider = provider;
            _db = db;
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task modules()
        {

            var settings = _db.Settings.FirstOrDefault();
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor("📚 Модули бота")
                .WithFooter($"Команды модуля - {settings.Prefix}commands [Модуль]");

            var mdls = _service.Modules;

            var emojis = new Dictionary<string, IEmote>
                {
                    { "Admin", new Emoji("⚠️") },
                    { "Moderator", new Emoji("⚜️") },
                    { "Help", new Emoji("📓") },
                    { "NsfwGif", new Emoji("🎀") },
                    { "SfwGif", new Emoji("🎎") },
                    { "User", new Emoji("🎃") },
                    { "Settings", new Emoji("⛓") },
                    { "Iventer", new Emoji("🎉") },
                };

            foreach (var mdl in mdls)
            {
                emojis.TryGetValue(mdl.Name, out IEmote emoji);
                var Permission = await mdl.GetExecutableCommandsAsync(Context, _provider);
                if (Permission.Count > 0)
                {
                    string name = mdl.Name;
                    if (emoji != null)
                        name = emoji.Name + " " + name;

                    emb.AddField(name, mdl.Summary ?? "test", true);
                }
            }
            if (emb.Fields.Count == 0)
                emb.WithDescription("Модули бота отсутствуют!");

            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task commands(string modules)
        {

            var settings = _db.Settings.FirstOrDefault();
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithDescription("")
                .WithAuthor($"📜 {modules} - Команды [префикс - {settings.Prefix}]");

            var mdls = _service.Modules.FirstOrDefault(x => x.Name.ToLower() == modules.ToLower());
            //if (mdls != null)
            //{
            //    var SuccessCommands = await mdls.GetExecutableCommandsAsync(Context, _provider);
            //    foreach (var Command in SuccessCommands)
            //        emb.Description += $"• {Command.Aliases.Last()} [{Command.Aliases[0]}]\n";

            //    emb.WithFooter($"Подробная информация о команде - {settings.Prefix}info [Имя команды]");
            //}
            //else 
            //    emb.WithDescription($"Модуль {modules} не найден!")
            //       .WithAuthor($"📜{modules} - ошибка");





            if (mdls != null && mdls.GetExecutableCommandsAsync(Context, _provider).Result.Count > 0)
            {
                var CommandList = new List<Initiliaze.Commands>();
                foreach (var lcomm in Initiliaze.ListCommand)
                {
                    foreach (var mcomm in mdls.Commands)
                    {
                        if (lcomm.Usage[1] == mcomm.Aliases[0])
                        {
                            var CommandAvialable = await mcomm.CheckPreconditionsAsync(Context, _provider);
                            if (CommandAvialable.IsSuccess)
                            {
                                CommandList.Add(lcomm);
                            }
                        }
                    }
                }
                CommandList = CommandList.OrderBy(x => x.Category).ToList();

                string TextCommand = string.Empty;
                foreach (var Command in CommandList)
                {
                    if (!string.IsNullOrWhiteSpace(Command.Category) && CommandList.Count(x => x.Category == Command.Category) > 1)
                    {
                        if (TextCommand == string.Empty || TextCommand != Command.Category)
                        {
                            TextCommand = Command.Category;
                            emb.AddField($"**{TextCommand}**\n", "⠀", true);
                        }
                        if (emb.Fields.Last().Value.ToString().Length == 1)
                            emb.Fields.Last().Value = $"• {Command.Usage[1]}\n";
                        else
                            emb.Fields.Last().Value += $"• {Command.Usage[1]}\n";
                    }
                    else
                    {
                        TextCommand = string.Empty;
                        emb.Description += $"• {Command.Usage[1]}\n";
                    }
                }
                if (emb.Description.Length > 0 && emb.Fields.Count > 0)
                {
                    emb.Description = emb.Description.Insert(0, "📚**Остальные команды**\n");
                }
                emb.WithFooter($"Подробная информация о команде - {settings.Prefix}info [Имя команды]");
            }
            else
                emb.WithDescription($"Модуль {modules} не найден!")
                   .WithAuthor($"📜{modules} - ошибка");


            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task info(string command)
        {
            command = command.ToLower();
            var Command = _service.Commands.FirstOrDefault(x => x.Aliases[0] == command || x.Aliases.Last() == command);
            var emb = new EmbedBuilder()
                .WithAuthor($"📋 Информация о {command}")
                .WithColor(BotSettings.DiscordColor);

            if (Command != null)
            {
                var settings = _db.Settings.FirstOrDefault();
                string text = string.Empty;
                foreach (var Parameter in Command.Parameters)
                {
                    text += $"[{Parameter}{(Parameter.IsOptional ? "/null" : "")}] ";
                }
                emb.AddField($"Сокращение: {string.Join(",", Command.Aliases)}",
                             $"Описание: {Command.Summary}\n" +
                             $"Пример: {settings.Prefix}{Command.Name} {text}");
            }
            else
                emb.WithDescription($"Команда `{command}` не найдена!");

            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }
    }
}
