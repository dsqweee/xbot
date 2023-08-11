//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Discord;
//using Discord.Commands;
//using Fergun.Interactive;
//using Fergun.Interactive.Pagination;
//using XBOT.Services.Configuration;

//namespace XBOT.Modules.Interactive;

//public class PaginatorModule : ModuleBase<SocketCommandContext>
//{
//    private readonly InteractiveService _interactive;

//    public PaginatorModule(InteractiveService interactive)
//    {
//        _interactive = interactive;
//    }

//    // Sends a message that contains a static paginator with pages that can be changed with reactions or buttons.
//    [Command("static", RunMode = RunMode.Async)]
//    public async Task PaginatorAsync()
//    {
//        var pages = new[]
//        {
//            new PageBuilder().WithDescription("Lorem ipsum dolor sit amet, consectetur adipiscing elit."),
//            new PageBuilder().WithDescription("Praesent eu est vitae dui sollicitudin volutpat."),
//            new PageBuilder().WithDescription("Etiam in ex sed turpis imperdiet viverra id eget nunc."),
//            new PageBuilder().WithDescription("Donec eget feugiat nisi. Praesent faucibus malesuada nulla, a vulputate velit eleifend ut.")
//        };

//        var paginator = new StaticPaginatorBuilder().WithActionOnTimeout(ActionOnStop.DeleteInput)
//            .AddUser(Context.User) // Only allow the user that executed the command to interact with the selection.
//            .WithPages(pages); // Set the pages the paginator will use. This is the only required component.



//        // Send the paginator to the source channel and wait until it times out after 10 minutes.
//        var message = await _interactive.SendPaginatorAsync(paginator.Build(), Context.Channel, new TimeSpan(0,0,30));
//        // By default, SendPaginatorAsync sends the paginator and waits for a timeout or a cancellation.
//        // If you want the method to return after sending the paginator, you can set the
//        // ReturnAfterSendingPaginator option to true in the InteractiveService configuration, InteractiveConfig.

//        // Example in ServiceCollection:
//        /*
//        var collection = new ServiceCollection()
//            .AddSingleton<DiscordSocketClient>()
//            .AddSingleton(new InteractiveConfig { ReturnAfterSendingPaginator = true })
//            .AddSingleton<InteractiveService>()
//            ...
//        */
//    }

//    // Sends a lazy paginator. The pages are generated using a page factory.
//    [Command("lazy", RunMode = RunMode.Async)]
//    public async Task LazyPaginatorAsync()
//    {
//        var paginator = new LazyPaginatorBuilder()
//            .AddUser(Context.User)
//            .WithPageFactory(GeneratePage) // The pages are now generated on demand using a local method.
//            .WithMaxPageIndex(9) // You must specify the max. index the page factory can go. max. index 9 = 10 pages
//            .Build();

//        await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(10));

//        static PageBuilder GeneratePage(int index)
//        {
//            return new PageBuilder()
//                .WithDescription($"This is page {index + 1}.")
//                .WithColor(BotSettings.DiscordColor);
//        }
//    }

//}