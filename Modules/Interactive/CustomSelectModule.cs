//using Fergun.Interactive;
//using Fergun.Interactive.Selection;
//using XBOT.DataBase;
//using XBOT.Services;
//using XBOT.Services.Configuration;

//namespace ExampleBot.Modules;

//public partial class CustomModuleX : ModuleBase<SocketCommandContext>
//{
//    private readonly CommandService _commandService;
//    private readonly InteractiveService _interactive;
//    private readonly IServiceProvider _serviceProvider;

//    public CustomModuleX(CommandService commandService, InteractiveService interactive, IServiceProvider serviceProvider)
//    {
//        _commandService = commandService;
//        _interactive = interactive;
//        _serviceProvider = serviceProvider;

//    }

//    // Sends a multi selection (a message with multiple select menus with options)
//    [Command("select", RunMode = RunMode.Async)]
//    public async Task MultiSelectionAsync()
//    {
//        using var _db = new db();
//        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));

//        var modules = _commandService.Modules.Where(x=>x.Commands.Count > 0).ToList();

//        var settings = db.Settings.FirstOrDefault();

//        var emb = new PageBuilder()
//            .WithColor(BotSettings.DiscordColor)
//            .WithAuthor("📚 Модули бота");

//        var emojis = new Dictionary<string, IEmote>
//                {
//                    { "Admin", new Emoji("⚠️") },
//                    { "Moderator", new Emoji("⚜️") },
//                    { "Help", new Emoji("📓") },
//                    { "NsfwGif", new Emoji("🎀") },
//                    { "SfwGif", new Emoji("🎎") },
//                    { "User", new Emoji("🎃") },
//                    { "Settings", new Emoji("⛓") },
//                    { "Iventer", new Emoji("🎉") },
//                };

//        foreach (var mdl in modules)
//        {
//            emojis.TryGetValue(mdl.Name, out IEmote emoji);
//            var Permission = await mdl.GetExecutableCommandsAsync(Context, _serviceProvider);
//            if (Permission.Count > 0)
//            {
//                string name = mdl.Name;
//                if (emoji != null)
//                    name = emoji.Name + " " + name;

//                emb.AddField(name, mdl.Summary ?? "test", true);
//            }
//            else
//                modules.Remove(mdl);
//        }
//        if (emb.Fields.Count == 0)
//            emb.WithDescription("Модули бота отсутствуют!");


//        // Used to track the selected module
//        string selectedModule = null;

//        IUserMessage message = null;
//        InteractiveMessageResult<MultiSelectionOption<string>> result = null;

//        // This timeout page is used for both cancellation (from the cancellation token) and timeout (specified in the SendSelectionAsync method).
//        var timeoutPage = new PageBuilder()
//            .WithDescription("Timeout!")
//            .WithColor(BotSettings.DiscordColor);

//        // Here we will use a "menu" message, that is, it can be reused multiple times until a timeout is received.
//        // In this case, we will reuse the message with the multi selection until we get a result (multi selection option) with a row value of 1.
//        // This can only happen if the result is successful and an option in the second select menu (the one that contains the commands) in selected.
//        while (result is null || result.IsSuccess && result.Value?.Row != 1)
//        {
//            // A multi selection uses the Row property of MultiSelectionOption to determine the position (the select menu) the options will appear.
//            // The modules will appear in the first select menu.
//            // There's also an IsDefault property used to determine whether an option is the default for a specific row.
//            var options = modules.Select(x => new MultiSelectionOption<string>(option: x.Name, row: 0, isDefault: x.Name == selectedModule));

//            string description = "Select a module";

//            // result will be null until a module is selected once.
//            // we know result won't be a command here because of the condition in the while loop.
//            if (result != null)
//            {
//                description = "Select a command\nNote: You can also update your selected module.";
//                var commands = modules
//                    .First(x => x.Name == result.Value!.Option)
//                    .Commands
//                    .Select(x => new MultiSelectionOption<string>(x.Name, 1));

//                options = options.Concat(commands);
//            }



//            var multiSelection = new MultiSelectionBuilder<string>()
//                .WithSelectionPage(emb)
//                .WithTimeoutPage(timeoutPage)
//                .WithCanceledPage(timeoutPage)
//                .WithActionOnTimeout(ActionOnStop.ModifyMessage | ActionOnStop.DeleteInput)
//                .WithActionOnCancellation(ActionOnStop.ModifyMessage | ActionOnStop.DeleteInput)
//                .WithOptions(options.ToArray())
//                .WithStringConverter(x => x.Option)
//                .AddUser(Context.User)
//                .Build();

//            result = await _interactive.SendSelectionAsync(multiSelection, message == null ? Context.Channel : message.Channel, TimeSpan.FromMinutes(2), null, cts.Token);

//            message = result.Message;

//            if (result.IsSuccess && result.Value!.Row == 0)
//            {
//                // We need to track the selected module so we can set it as the default option.
//                selectedModule = result.Value!.Option;
//            }
//        }

//        if (!result.IsSuccess)
//            return;

//        var embed = new EmbedBuilder()
//            .WithDescription($"You selected:\n**Module**: {selectedModule}\n**Command**: {result.Value!.Option}")
//            .WithColor(BotSettings.DiscordColor)
//            .Build();

//        await message.ModifyAsync(x =>
//        {
//            x.Embed = embed;
//            x.Components = new ComponentBuilder().Build(); // Remove components
//        });
//    }
//}

