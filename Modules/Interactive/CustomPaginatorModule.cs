//using System.Collections.ObjectModel;
//using Fergun.Interactive;
//using Fergun.Interactive.Extensions;
//using Fergun.Interactive.Pagination;
//using Fergun.Interactive.Selection;

//namespace ExampleBot.Modules;

//public partial class CustomModulezxc : ModuleBase<SocketCommandContext>
//{
//    private readonly InteractiveService _interactive;

//    public CustomModulezxc(InteractiveService interactive)
//    {
//        _interactive = interactive;
//    }
//    // Sends a paginated (paged) selection
//    // A paged selection is a selection where each options contains a paginator
//    // Here, 3 different image scrapers are used to get images, then their results are grouped in a selection.
//    [Command("paginator", RunMode = RunMode.Async)]
//    public async Task PagedSelection2Async(string query = "Discord")
//    {
//        await Context.Channel.TriggerTypingAsync();

//        var results = new Dictionary<string, List<string>>
//        {
//            { "Google", new List<string>{ "https://www.searchenginejournal.com/wp-content/uploads/2022/06/image-search-1600-x-840-px-62c6dc4ff1eee-sej-1280x720.png", "https://www.seiu1000.org/sites/main/files/main-images/camera_lense_0.jpeg" } },
//            { "DuckDuckGo", new List<string>{ "https://www.simplilearn.com/ice9/free_resources_article_thumb/what_is_image_Processing.jpg", "https://images.ctfassets.net/hrltx12pl8hq/7yQR5uJhwEkRfjwMFJ7bUK/dc52a0913e8ff8b5c276177890eb0129/offset_comp_772626-opt.jpg?fit=fill&w=800&h=300" } }
//        };

//        var options = results
//            .Where(x => x.Value.Count > 0)
//            .ToDictionary(x => x.Key, x => new LazyPaginatorBuilder()
//                .WithPageFactory(index => GeneratePage(x.Value, x.Key, index))
//                .WithMaxPageIndex(x.Value.Count - 1)
//                .WithActionOnCancellation(ActionOnStop.DisableInput)
//                .WithActionOnTimeout(ActionOnStop.DisableInput)
//                .WithFooter(PaginatorFooter.PageNumber)
//                .AddUser(Context.User)
//                //.WithCustomEmotes()
//                .Build() as Paginator);

//        if (options.Count == 0)
//        {
//            await ReplyAsync("No results.");
//            return;
//        }

//        // We have to provide an initial page to the selection, there's no easy way do to this within the selection
//        string first = options.First().Key;
//        var initialPage = GeneratePage(results[first], first, 0);

//        var pagedSelection = new PagedSelectionBuilder<string>()
//            .WithOptions(options)
//            .AddUser(Context.User)
//            .WithSelectionPage(initialPage)
//            .WithActionOnTimeout(ActionOnStop.DisableInput)
//            .WithActionOnCancellation(ActionOnStop.DisableInput)
//            .Build();

//        await _interactive.SendSelectionAsync(pagedSelection, Context.Channel, TimeSpan.FromMinutes(10));

//        PageBuilder GeneratePage(List<string> images, string scraper, int index)
//        {
//            return new PageBuilder()
//                .WithAuthor(Context.User)
//                .WithTitle("123")
//                .WithDescription($"{scraper} Images")
//                .WithImageUrl(images[index])
//                .WithFooter($"Page {index + 1}/{images.Count}");
//        }
//    }
//}
//public class PagedSelectionBuilder<TOption> : BaseSelectionBuilder<PagedSelection<TOption>, KeyValuePair<TOption, Paginator>, PagedSelectionBuilder<TOption>>
//{
//    public PagedSelection<TOption> Build(PageBuilder startPage)
//    {
//        SelectionPage = startPage;
//        return Build();
//    }

//    /// <inheritdoc />
//    public override PagedSelection<TOption> Build()
//    {
//        base.Options = Options;
//        return new PagedSelection<TOption>(this);
//    }

//    /// <summary>
//    /// Gets a dictionary of options and their paginators.
//    /// </summary>
//    public new IDictionary<TOption, Paginator> Options { get; set; } = new Dictionary<TOption, Paginator>();

//    public override Func<KeyValuePair<TOption, Paginator>, string> StringConverter { get; set; } = option => option.Key?.ToString();

//    public PagedSelectionBuilder<TOption> WithOptions<TPaginator>(IDictionary<TOption, TPaginator> options) where TPaginator : Paginator
//    {
//        Options = options as IDictionary<TOption, Paginator> ?? throw new ArgumentNullException(nameof(options));
//        return this;
//    }

//    public PagedSelectionBuilder<TOption> AddOption(TOption option, Paginator paginator)
//    {
//        Options.Add(option, paginator);
//        return this;
//    }
//}

//public class PagedSelection<TOption> : BaseSelection<KeyValuePair<TOption, Paginator>>
//{
//    /// <inheritdoc />
//    public PagedSelection(PagedSelectionBuilder<TOption> builder) : base(builder)
//    {
//        Options = new ReadOnlyDictionary<TOption, Paginator>(builder.Options);
//        CurrentOption = Options.Keys.First();
//    }

//    /// <summary>
//    /// Gets a dictionary of options and their paginators.
//    /// </summary>
//    public new IReadOnlyDictionary<TOption, Paginator> Options { get; }

//    /// <summary>
//    /// Gets the current option.
//    /// </summary>
//    public TOption CurrentOption { get; private set; }

//    public override ComponentBuilder GetOrAddComponents(bool disableAll, ComponentBuilder builder = null)
//    {
//        builder ??= new ComponentBuilder();
//        var paginator = Options[CurrentOption];

//        // add paginator components to the builder
//        paginator.GetOrAddComponents(disableAll, builder);

//        // select menu
//        var options = new List<SelectMenuOptionBuilder>();

//        foreach (var selection in Options)
//        {
//            var emote = EmoteConverter?.Invoke(selection);
//            string label = StringConverter?.Invoke(selection);
//            if (emote is null && label is null)
//            {
//                throw new InvalidOperationException($"Neither {nameof(EmoteConverter)} nor {nameof(StringConverter)} returned a valid emote or string.");
//            }

//            var option = new SelectMenuOptionBuilder()
//                .WithLabel(label)
//                .WithEmote(emote)
//                .WithDefault(Equals(selection.Key, CurrentOption))
//                .WithValue(emote?.ToString() ?? label);

//            options.Add(option);
//        }

//        var selectMenu = new SelectMenuBuilder()
//            .WithCustomId("foobar")
//            .WithOptions(options)
//            .WithDisabled(disableAll);

//        builder.WithSelectMenu(selectMenu);

//        return builder;
//    }

//    public override async Task<InteractiveInputResult<KeyValuePair<TOption, Paginator>>> HandleInteractionAsync(SocketMessageComponent input, IUserMessage message)
//    {
//        if (input.Message.Id != message.Id || !this.CanInteract(input.User))
//        {
//            return InteractiveInputStatus.Ignored;
//        }

//        string option = input.Data.Values?.FirstOrDefault();

//        if (input.Data.Type == ComponentType.SelectMenu && option is not null)
//        {
//            KeyValuePair<TOption, Paginator> selected = default;
//            string selectedString = null;

//            foreach (var value in Options)
//            {
//                string stringValue = EmoteConverter?.Invoke(value)?.ToString() ?? StringConverter?.Invoke(value);
//                if (option != stringValue) continue;
//                selected = value;
//                selectedString = stringValue;
//                break;
//            }

//            if (selectedString is null)
//            {
//                return InteractiveInputStatus.Ignored;
//            }

//            CurrentOption = selected.Key;

//            bool isCanceled = AllowCancel && (EmoteConverter?.Invoke(CancelOption)?.ToString() ?? StringConverter?.Invoke(CancelOption)) == selectedString;

//            if (isCanceled)
//            {
//                return new(InteractiveInputStatus.Canceled, selected);
//            }
//        }

//        var paginator = Options[CurrentOption];

//        var action = (PaginatorAction)(input.Data.CustomId?[^1] - '0' ?? -1);

//        if (Enum.IsDefined(typeof(PaginatorAction), action))
//        {
//            if (action == PaginatorAction.Exit)
//            {
//                return InteractiveInputStatus.Canceled;
//            }

//            await paginator.ApplyActionAsync(action).ConfigureAwait(false);
//        }

//        var currentPage = await paginator.GetOrLoadCurrentPageAsync().ConfigureAwait(false);
//        var attachments = currentPage.AttachmentsFactory is null ? null : await currentPage.AttachmentsFactory().ConfigureAwait(false);

//        await input.UpdateAsync(x =>
//        {
//            x.Content = currentPage.Text ?? "";
//            x.Embeds = currentPage.GetEmbedArray();
//            x.Components = GetOrAddComponents(false).Build();
//            x.AllowedMentions = currentPage.AllowedMentions;
//            x.Attachments = attachments is null ? new Optional<IEnumerable<FileAttachment>>() : new Optional<IEnumerable<FileAttachment>>(attachments);
//        }).ConfigureAwait(false);

//        return InteractiveInputStatus.Ignored;
//    }
//}