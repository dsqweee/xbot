//using Discord.Interactions;
//using XBOT.Services;
//using XBOT.Services.Attribute.CommandList;
//using XBOT.Services.Configuration;

//namespace XBOT.Modules.Interaction
//{
//    [Discord.Interactions.Group("help", "Модули по передвижению по боту")]
//    public class HelpModule : InteractionModuleBase<SocketInteractionContext>
//    {
//        private readonly CommandService _service;
//        private readonly IServiceProvider _provider;
//        private readonly InteractionService _interactionService;
//        private ComponentEventService _componentEventService;


//        public HelpModule(CommandService service, IServiceProvider provider, InteractionService interactionService, ComponentEventService componentEventService)
//        {
//            _service = service;
//            _provider = provider;
//            _interactionService = interactionService;
//            _componentEventService = componentEventService;
//        }


//        private Dictionary<string, IEmote> List = new Dictionary<string, IEmote> { { "User", new Emoji("🎃") }, { "Help", new Emoji("📓") } };

//        [SlashCommand("modules", "открыть модули бота")]
//        public async Task modules()
//        {
//            using (Db _db = new ())
//            {
//                var Prefix = db.Settings.FirstOrDefault().Prefix;

//                var emb = new EmbedBuilder()
//                    .WithColor(BotSettings.DiscordColor)
//                    .WithAuthor("📚 Модули бота")
//                    .WithFooter($"Окно активно 180 секунд. Повторный вызов - /help modules");

//                var MenuGuidId = Guid.NewGuid().ToString();
//                var menuBuilder = new SelectMenuBuilder()
//                    .WithCustomId(MenuGuidId)
//                    .WithPlaceholder("Выберите модуль");

//                var userInteraction = new ComponentEvent<SocketMessageComponent>(new HashSet<string> { MenuGuidId });
//                _componentEventService.AddInteraction(new HashSet<string> { MenuGuidId }, userInteraction);

//                ModulesComamnd(ref menuBuilder, ref emb);

//                var builder = new ComponentBuilder().WithSelectMenu(menuBuilder);
//                await ReplyAsync("", embed: emb.Build(), components: builder.Build());

//                var selectedOption = await userInteraction.WaitForInteraction();
//                InteractionDispose();

//                var Module = selectedOption.Data.Values.First();
//                emb.WithAuthor($"📜 Команды модуля {Module}");
//                var Guild = db.Settings.FirstOrDefault();
//                emb.Footer.Text += $"\nВы можете написать команду вручную - {Guild.Prefix}userinfo";
//                MenuGuidId = Guid.NewGuid().ToString();
//                menuBuilder.WithCustomId(MenuGuidId).WithPlaceholder("Выберите команду");

//                userInteraction = new ComponentEvent<SocketMessageComponent>(new HashSet<string> { MenuGuidId });
//                _componentEventService.AddInteraction(new HashSet<string> { MenuGuidId }, userInteraction);
//                emb.Fields.Clear();
//                menuBuilder.Options.Clear();
//                CommandsList(Module, ref menuBuilder, ref emb);

//                builder = new ComponentBuilder().WithSelectMenu(menuBuilder);
//                await Context.Interaction.ModifyOriginalResponseAsync(x =>{ x.Components = builder.Build(); x.Embed = emb.Build(); });

//                selectedOption = await userInteraction.WaitForInteraction();
//                InteractionDispose();

//                var Command = selectedOption.Data.Values.First();
//                emb.WithAuthor($"📋 Информация о {Command}");
//                emb.WithFooter($"\nВы можете написать команду вручную - {Guild.Prefix}.i [имя команды]");
//                emb.Fields.Clear();
//                CommandInfo(Command, Prefix, ref emb);
//                await Context.Interaction.ModifyOriginalResponseAsync(x => { x.Components = new ComponentBuilder().Build(); x.Embed = emb.Build(); });

//                async void InteractionDispose()
//                {
//                    if (selectedOption == null)
//                    {
//                        _componentEventService.RemoveInteraction(MenuGuidId);
//                        await Context.Interaction.ModifyOriginalResponseAsync(x => x.Components = new ComponentBuilder().Build());
//                        return;
//                    }
//                    _componentEventService.RemoveInteraction(MenuGuidId);
//                }
//            }
//        }

//        private void ModulesComamnd(ref SelectMenuBuilder menuBuilder, ref EmbedBuilder emb)
//        {
//            foreach (var mdl in _service.Modules)
//            {
//                var Permission = mdl.GetExecutableCommandsAsync(Context as ICommandContext, _provider);

//                if (Permission.Result.Count > 0)
//                {
//                    var Emoji = List.GetValueOrDefault(mdl.Name);

//                    string Name = $"{Emoji} {mdl.Name}";
//                    menuBuilder.AddOption(mdl.Name, mdl.Name, null, Emoji);
//                    emb.AddField(Name, $"{mdl.Summary}\n", true);
//                }
//            }
//        }

//        private void CommandsList(string modules,ref SelectMenuBuilder menuBuilder, ref EmbedBuilder emb)
//        {
//            using (Db _db = new ())
//            {
                
//                var mdls = _service.Modules.FirstOrDefault(x => x.Name.ToLower() == modules.ToLower());
//                var ValidCommands = mdls.GetExecutableCommandsAsync(Context as ICommandContext, _provider);
//                if (mdls != null && ValidCommands.Result.Count > 0)
//                {
//                    var CommandList = Initiliaze.ListCommand.Where(z => mdls.Commands.Any(x => x.Aliases[0] == z.Usage[1] && x.CheckPreconditionsAsync(Context as ICommandContext, _provider).Result.IsSuccess)).OrderBy(x => x.Category).ToList();
//                    string TextCommand = string.Empty;
//                    foreach (var Command in CommandList)
//                    {
//                        if (!string.IsNullOrWhiteSpace(Command.Category) && CommandList.Count(x => x.Category == Command.Category) > 1)
//                        {
//                            if (TextCommand == string.Empty || TextCommand != Command.Category)
//                            {
//                                TextCommand = Command.Category;
//                                emb.AddField($"**{TextCommand}**\n", "⠀", true);
//                            }
//                            if (emb.Fields.Last().Value.ToString().Length == 1)
//                                emb.Fields.Last().Value = $"• {Command.Usage[1]}\n";
//                            else
//                                emb.Fields.Last().Value += $"• {Command.Usage[1]}\n";
                            
//                        }
//                        else
//                        {
//                            TextCommand = string.Empty;
//                            emb.Description += $"• {Command.Usage[1]}\n";
//                        }
//                        menuBuilder.AddOption(Command.Usage[1], Command.Usage[1]);
//                    }
//                    if (emb.Description.Length > 0 && emb.Fields.Count > 0)
//                    {
//                        emb.Description = emb.Description.Insert(0, "📚**Остальные команды**\n");
//                    }
//                    var Guild = db.Settings.FirstOrDefault();
//                    emb.WithFooter($"Подробная информация о команде - {Guild.Prefix}i [Имя команды]");
//                }
//                else
//                {
//                    emb.WithDescription($"Модуль {modules} не найден!").WithAuthor($"📜{modules} - ошибка");
//                }
                    
//            }
//        }

//        private void CommandInfo(string CommandName, string prefix, ref EmbedBuilder emb)
//        {
//            var Command = _service.Commands.FirstOrDefault(x => x.Aliases[0].ToLower() == CommandName || x.Aliases.Last().ToLower() == CommandName);
//            string text = string.Empty;

//            if(Command.Parameters != null)
//                foreach (var Parameter in Command.Parameters)
//                {
//                    text += $"[{Parameter}{(Parameter.IsOptional ? "/ничего" : "")}] ";
//                }

//            emb.AddField($"Сокращение: {Command.Remarks.Replace('"', ' ')}",
//                         $"Описание: {Command.Summary}\n" +
//                         $"Пример: {prefix}{Command.Name} {text}");
//        }

//        //[SlashCommand("commands", "открыть команды модуля")]
//        //public async Task commands(string modules)
//        //{
//        //    using (Db _db = new ())
//        //    {
//        //        var Guild = db.Settings.FirstOrDefault();
//        //        var emb = new EmbedBuilder().WithColor(255, 0, 94).WithDescription("").WithAuthor($"📜 {modules} - Команды [префикс - {Guild.Prefix}]");

//        //        var mdls = _service.Modules.FirstOrDefault(x => x.Name.ToLower() == modules.ToLower());
//        //        if (mdls != null && mdls.GetExecutableCommandsAsync(Context as ICommandContext, _provider).Result.Count > 0)
//        //        {
//        //            var CommandList = Initiliaze.ListCommand.Where(z => mdls.Commands.Any(x => x.Aliases[0] == z.Usage[1] && x.CheckPreconditionsAsync(Context as ICommandContext, _provider).Result.IsSuccess)).OrderBy(x => x.Category).ToList();
//        //            string TextCommand = string.Empty;
//        //            foreach (var Command in CommandList)
//        //            {
//        //                if (!string.IsNullOrWhiteSpace(Command.Category) && CommandList.Count(x => x.Category == Command.Category) > 1)
//        //                {
//        //                    if (TextCommand == string.Empty || TextCommand != Command.Category)
//        //                    {
//        //                        TextCommand = Command.Category;
//        //                        emb.AddField($"**{TextCommand}**\n", "⠀", true);
//        //                    }
//        //                    if (emb.Fields.Last().Value.ToString().Length == 1)
//        //                        emb.Fields.Last().Value = $"• {Command.Usage[1]}\n";
//        //                    else
//        //                        emb.Fields.Last().Value += $"• {Command.Usage[1]}\n";
//        //                }
//        //                else
//        //                {
//        //                    TextCommand = string.Empty;
//        //                    emb.Description += $"• {Command.Usage[1]}\n";
//        //                }
//        //            }
//        //            if (emb.Description.Length > 0 && emb.Fields.Count > 0)
//        //            {
//        //                emb.Description = emb.Description.Insert(0, "📚**Остальные команды**\n");
//        //            }
//        //            emb.WithFooter($"Подробная информация о команде - {Guild.Prefix}i [Имя команды]");
//        //        }
//        //        else emb.WithDescription($"Модуль {modules} не найден!").WithAuthor($"📜{modules} - ошибка");
//        //        await ReplyAsync("", false, emb.Build());
//        //    }
//        //}

//        [SlashCommand("info", "открыть информацию о команде")]
//        public async Task info(string command)
//        {
//            using (Db _db = new ())
//            {
//                command = command.ToLower();
//                var Command = _service.Commands.FirstOrDefault(x => x.Aliases[0].ToLower() == command || x.Aliases.Last().ToLower() == command);
//                var emb = new EmbedBuilder().WithAuthor($"📋 Информация о {command}").WithColor(255, 0, 94);

//                if (Command != null)
//                {
//                    string text = string.Empty;
//                    if(Command.Parameters != null)
//                        foreach (var Parameter in Command.Parameters)
//                        {
//                            text += $"[{Parameter}{(Parameter.IsOptional ? "/можно не указывать" : "")}] ";
//                        }
//                    var prefix = db.Settings.FirstOrDefault().Prefix;
//                    emb.AddField($"Сокращение: {Command.Remarks.Replace('"', ' ')}",
//                                 $"Описание: {Command.Summary}\n" +
//                                 $"Пример: {prefix}{Command.Name} {text}");
//                }
//                else
//                    emb.WithDescription($"Команда `{command}` не найдена!");
//                await ReplyAsync("", false, emb.Build());
//            }
//        }
//    }
//}
