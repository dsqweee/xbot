﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Discord;
//using Discord.Commands;
//using Fergun.Interactive;
//using Fergun.Interactive.Selection;
//using XBOT.Services;
//using XBOT.Services.Configuration;
//using static XBOT.Services.Handling.CommandHandlingService;

//namespace ExampleBot.Modules;

//public class SelectionModule : ModuleBase<SocketCommandContext>
//{
//    private readonly InteractiveService _interactive;

//    public SelectionModule(InteractiveService interactive)
//    {
//        _interactive = interactive;
//    }

//    Sends a message that contains selection of options which the user can select one.
//    Here the selection is received via messages.
//    Important: Commands using methods from InteractiveService should always have RunMode.Async

//   [Command("simple")]
//    public async Task SelectAsync()
//{
//    var options = new[] { "C", "C++", "C#", "Java", "Python", "JavaScript", "PHP" };

//    var pageBuilder = new PageBuilder() // A PageBuilder is just an EmbedBuilder with a Text property (content).
//        .WithTitle("Selection Example")
//        .WithDescription($"Select a programming language:\n{string.Join('\n', options.Select(x => $"- **{x}**"))}");
//    Random embed color.

//   var selection = new SelectionBuilder<string>() // Create a new SelectionBuilder that uses strings as input.
//       .AddUser(Context.User) // Only allow the user that executed the command to interact with the selection.
//       .WithOptions(options) // Set the options.
//       .WithInputType(InputType.Messages) // Use messages to receive the selection.
//       .WithSelectionPage(pageBuilder) // Set the selection page, this is the page that will be displayed in the message.
//       .WithDeletion(DeletionOptions.None) // By default, the selections delete valid inputs (messages, reactions) but you can change this behaviour.
//       .Build(); // Build the SelectionBuilder.

//    Send the selection to the source channel and wait until it times out
//        var result = await _interactive.SendSelectionAsync(selection, Context.Channel, TimeSpan.FromMinutes(1));

//    Get the selected option. This may be null / default if the selection times out or gets cancelled.
//        string selected = result.Value;

//    You can check whether a selection failed or ended successfully with the Status property.
//         var status = result.Status;

//    You can also use the IsSuccess, IsCanceled and IsTimeout properties.
//        bool isSuccess = result.IsSuccess;

//    var builder = new EmbedBuilder()
//        .WithDescription(isSuccess ? $"You selected: {selected}" : "Timeout!");

//    await ReplyAsync(embed: builder.Build());
//}

//Sends a emote selection.An emote selection is built using an EmoteSelectionBuilder.
//This variant of SelectionBuilder just exists for convenience,
//     but makes creating selections using reactions or buttons much easier.
//    [Command("emote")]
//    public async Task EmoteAsync()
//{
//    var emotes = new[]
//    {
//            new Emoji("📱"),
//            new Emoji("🖥"),
//            new Emoji("💻")
//        };

//    var pageBuilder = new PageBuilder()
//        .WithDescription("Select the device you're using");

//    var selection = new EmoteSelectionBuilder()
//        .AddUser(Context.User)
//        .WithOptions(emotes)
//        .WithSelectionPage(pageBuilder)
//             Normally you would need to specify an EmoteConverter, but EmoteSelectionBuilder has already implemented a default converter for emotes.
//            .WithEmoteConverter(x => x)
//             There's no need to specify an input type since the builder now uses the most appropriate one (reactions/buttons) by default
//            .WithInputType(InputType.Reactions)
//            .Build();

//        var result = await _interactive.SendSelectionAsync(selection, Context.Channel, TimeSpan.FromMinutes(1));

//    var builder = new EmbedBuilder()
//        .WithDescription(result.IsSuccess ? $"You selected: {result.Value}" : "Timeout!");

//    await ReplyAsync(embed: builder.Build());
//}

//[Command("button")]
//public async Task buttonAsync()
//{
//    var options = new ButtonOption<string>[]
//    {
//            new("Primary", ButtonStyle.Primary),
//            new("Secondary", ButtonStyle.Secondary),
//            new("Success", ButtonStyle.Success),
//            new("Danger", ButtonStyle.Danger)
//    };

//    var pageBuilder = new PageBuilder()
//        .WithDescription("Button selection");

//    var buttonSelection = new ButtonSelectionBuilder<string>()
//        .WithActionOnSuccess(ActionOnStop.DeleteInput)
//        .WithActionOnTimeout(ActionOnStop.DeleteInput)
//        .WithActionOnCancellation(ActionOnStop.DeleteInput)
//        .WithOptions(options)
//        .WithStringConverter(x => x.Option)
//        .WithSelectionPage(pageBuilder)
//        .AddUser(Context.User)
//        .Build();

//    var result = await _interactive.SendSelectionAsync(buttonSelection, Context.Channel);

//    var emote = result.Value;

//    var builder = new EmbedBuilder()
//        .WithDescription(result.IsSuccess ? $"You selected: {emote}" : "Timeout!");

//    await ReplyAsync(embed: builder.Build());
//}












//Sends an emote selection, where each emote represents a specific value.

//[Command("emote2")]
//    public async Task Emote2Async()
//{
//    var emotes = new Dictionary<IEmote, string>
//    {
//        [new Emoji("\u0031\u20E3")] = "one",
//        [new Emoji("\u0032\u20E3")] = "two",
//        [new Emoji("\u0033\u20E3")] = "three",
//        [new Emoji("\u0034\u20E3")] = "four",
//        [new Emoji("\u0035\u20E3")] = "five"
//    };

//    var pageBuilder = new PageBuilder()
//        .WithDescription("Select a number");

//    var selection = new EmoteSelectionBuilder<string>()
//        .AddUser(Context.User)
//        .WithOptions(emotes)
//        .WithSelectionPage(pageBuilder)
//        .Build();

//    var result = await _interactive.SendSelectionAsync(selection, Context.Channel, TimeSpan.FromMinutes(1));

//    In this case the result's value will be a KeyValuePair<IEmote, int>
//        var emote = result.Value.Key; // Selected emote
//        string selected = result.Value.Value; // Selected option

//        var builder = new EmbedBuilder()
//            .WithDescription(result.IsSuccess ? $"You selected: {emote} ({selected})" : "Timeout!");

//        await ReplyAsync(embed: builder.Build());
//    }

//    Sends a selection that can be canceled, disables / deletes its input(reactions/ buttons) after selecting an option,
//    modifies itself to a specific page after ending, and deletes itself after cancelling it.
//    [Command("extra", RunMode = RunMode.Async)]
//    public async Task ExtraAsync()
//    {
//        var items = new Item[]
//        {
//            new("Fruit", new Emoji("🍎")),
//            new("Vegetable", new Emoji("🥦")),
//            new("Fast food", new Emoji("🍔")),
//            new("Dessert", new Emoji("🍰")),
//            new("Dairy", new Emoji("🧀")),
//             Important: When using an option for cancellation(with WithAllowCancel(), shown below), the last option will be used to cancel the selection.
//            new("Cancel", new Emoji("❌"))
//        };

//var color = BotSettings.DiscordColor;

//var pageBuilder = new PageBuilder()
//    .WithDescription("Select a food type")
//    .WithColor(color);

//This page will be displayed when a valid input is received(except cancellations).
//        var successPage = new PageBuilder()
//            .WithDescription("Thank you for participating.")
//            .WithColor(color);

//This page will be displayed when a selection times out.
//        var timeoutPage = new PageBuilder()
//            .WithDescription("Timeout!")
//            .WithColor(color);

//var selection = new SelectionBuilder<Item>()
//    .AddUser(Context.User)
//    .WithOptions(items)
//    .WithSelectionPage(pageBuilder)
//    .WithSuccessPage(successPage)
//    .WithTimeoutPage(timeoutPage)
//             Tell the selection to modify the message to successPage and delete the input(reactions, buttons, select menu) if a valid input is received.
//            .WithActionOnSuccess(ActionOnStop.ModifyMessage | ActionOnStop.DeleteInput)
//             Tell the selection to modify the message to timeoutPage and delete the input if the selection times out.
//            .WithActionOnTimeout(ActionOnStop.ModifyMessage | ActionOnStop.DeleteInput)
//            .WithAllowCancel(true) // We need to tell the selection there's a cancel option.
//            .WithActionOnCancellation(ActionOnStop.DeleteMessage) // Delete the message if the selection gets canceled.
//             The Name property is used to get a string representation of an item.This is required in selections using messages and select menus as input.
//            .WithStringConverter(item => item.Name)
//             The Emote property is used to get an emote representation of an item.This is required in selections using reactions as input.
//            .WithEmoteConverter(item => item.Emote)
//             Since we have set both string and emote converters, we can use all 4 input types.
//            .WithInputType(InputType.Reactions | InputType.Messages | InputType.Buttons | InputType.SelectMenus)
//            .Build();

//await _interactive.SendSelectionAsync(selection, Context.Channel, TimeSpan.FromMinutes(1));
//    }

//     A menu of options that uses (and reuses) a selection message.
//    [Command("menu", RunMode = RunMode.Async)]
//public async Task MenuAsync()
//{
//    Create CancellationTokenSource that will be canceled after 10 minutes.
//   var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));

//    var options = new[]
//    {
//            "Cache messages",
//            "Cache users",
//            "Allow using mentions as prefix",
//            "Ignore command errors",
//            "Ignore command errors",
//            "Ignore command errors",
//            "Ignore command errors",
//            "Ignore command errors",
//            "Ignore command errors"

//        };

//    var values = new[]
//    {
//            true,
//            false,
//            true,
//            false,
//            true,
//            false,
//            false,
//            false,
//            false
//        };

//    Dynamically create the number emotes
//   var emotes = Enumerable.Range(1, options.Length)
//       .ToDictionary(x => new Emoji($"{x}\ufe0f\u20e3") as IEmote, y => y);

//    Add the cancel emote at the end of the dictionary
//        emotes.Add(new Emoji("❌"), -1);

//    var color = BotSettings.DiscordColor;

//    Prefer disabling the input(buttons, select menus) instead of removing them from the message.
//        var actionOnStop = ActionOnStop.DisableInput;

//    InteractiveMessageResult<KeyValuePair<IEmote, int>> result = null;
//    IUserMessage message = null;

//    while (result is null || result.Status == InteractiveStatus.Success)
//    {
//        var pageBuilder = new PageBuilder()
//            .WithTitle("Bot Control Panel")
//            .WithDescription("Use the reactions/buttons to enable or disable an option.")
//            .AddField("Option", string.Join('\n', options.Select((x, i) => $"**{i + 1}**. {x}")), true)
//            .AddField("Value", string.Join('\n', values), true)
//            .WithColor(color);

//        var selection = new EmoteSelectionBuilder<int>()
//            .AddUser(Context.User)
//            .WithSelectionPage(pageBuilder)
//            .WithOptions(emotes)
//            .WithAllowCancel(true)
//            .WithActionOnCancellation(actionOnStop)
//            .WithActionOnTimeout(actionOnStop)
//            .Build();

//        if message is null, SendSelectionAsync() will send a message, otherwise it will modify the message.
//             The cancellation token persists here, so it will be canceled after 10 minutes no matter how many times the selection is used.
//            result = message is null
//                ? await _interactive.SendSelectionAsync(selection, Context.Channel, TimeSpan.FromMinutes(10), cancellationToken: cts.Token)
//                : await _interactive.SendSelectionAsync(selection, message, TimeSpan.FromMinutes(10), cancellationToken: cts.Token);

//        Store the used message.
//            message = result.Message;

//        Break the loop if the result isn't successful
//            if (!result.IsSuccess) break;

//        int selected = result.Value.Value;

//        Invert the value of the selected option
//       values[selected - 1] = !values[selected - 1];

//        Do stuff with the selected option
//        }
//}

//private sealed record Item(string Name, IEmote Emote);
//}