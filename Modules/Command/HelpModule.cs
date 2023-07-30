using XBOT.Services.Attribute;
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

        public HelpModule(CommandService service, IServiceProvider provider)
        {
            _service = service;
            _provider = provider;
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task modules()
        {
            using (db _db = new ())
            {
                var settings = _db.Settings.FirstOrDefault();
                var emb = new EmbedBuilder()
                    .WithColor(BotSettings.DiscordColor)
                    .WithAuthor("📚 Модули бота")
                    .WithFooter($"Команды модуля - {settings.Prefix}commands [Модуль]");

                var mdls = _service.Modules;

                foreach (var mdl in mdls)
                {
                    var Permission = await mdl.GetExecutableCommandsAsync(Context, _provider);
                    if (Permission.Count > 0)
                    {
                        emb.AddField(mdl.Name, mdl?.Summary, true);
                    }
                }
                if (emb.Fields.Count == 0) 
                    emb.WithDescription("Модули бота отсутствуют!");

                await Context.Channel.SendMessageAsync("",false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task commands(string modules)
        {
            using (db _db = new ())
            {
                var settings = _db.Settings.FirstOrDefault();
                var emb = new EmbedBuilder()
                    .WithColor(BotSettings.DiscordColor)
                    .WithDescription("")
                    .WithAuthor($"📜 {modules} - Команды [префикс - {settings.Prefix}]");

                var mdls = _service.Modules.FirstOrDefault(x => x.Name.ToLower() == modules.ToLower());
                if (mdls != null)
                {
                    var SuccessCommands = await mdls.GetExecutableCommandsAsync(Context, _provider);
                    foreach (var Command in SuccessCommands)
                        emb.Description += $"• {Command.Aliases[1]} [{Command.Aliases[0]}]\n";
                    
                    emb.WithFooter($"Подробная информация о команде - {settings.Prefix}info [Имя команды]");
                }
                else 
                    emb.WithDescription($"Модуль {modules} не найден!")
                       .WithAuthor($"📜{modules} - ошибка");

                await Context.Channel.SendMessageAsync("",false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task info(string command)
        {
            using (db _db = new ())
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

                await Context.Channel.SendMessageAsync("", false ,emb.Build());
            }
        }
    }
}
