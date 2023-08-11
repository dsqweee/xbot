using Fergun.Interactive.Selection;
using Fergun.Interactive;

namespace XBOT.Services;

public class MultiSelectionBuilder<T> : BaseSelectionBuilder<MultiSelection<T>, MultiSelectionOption<T>, MultiSelectionBuilder<T>>
{
    public override InputType InputType => InputType.SelectMenus;

    public override MultiSelection<T> Build() => new(this);
}

public class MultiSelection<T> : BaseSelection<MultiSelectionOption<T>>
{
    public MultiSelection(MultiSelectionBuilder<T> builder)
        : base(builder)
    {
    }

    public override ComponentBuilder GetOrAddComponents(bool disableAll, ComponentBuilder builder = null)
    {
        builder ??= new ComponentBuilder();
        var selectMenus = new Dictionary<int, SelectMenuBuilder>();

        foreach (var option in Options)
        {
            if (!selectMenus.ContainsKey(option.Row))
            {
                selectMenus[option.Row] = new SelectMenuBuilder()
                    .WithCustomId($"selectmenu{option.Row}")
                    .WithDisabled(disableAll);
            }

            var emote = EmoteConverter?.Invoke(option);
            string label = StringConverter?.Invoke(option);
            if (emote is null && label is null)
            {
                throw new InvalidOperationException($"Neither {nameof(EmoteConverter)} nor {nameof(StringConverter)} returned a valid emote or string.");
            }

            string optionValue = emote?.ToString() ?? label;

            var optionBuilder = new SelectMenuOptionBuilder()
                .WithLabel(label)
                .WithEmote(emote)
                .WithValue(optionValue)
                .WithDefault(option.IsDefault);

            selectMenus[option.Row].AddOption(optionBuilder);
        }

        foreach ((int row, var selectMenu) in selectMenus)
        {
            builder.WithSelectMenu(selectMenu, row);
        }

        return builder;
    }
}

public class MultiSelectionOption<T>
{
    public MultiSelectionOption(T option, int row, ulong selectId, bool isDefault = false)
    {
        Option = option;
        Row = row;
        IsDefault = isDefault;
        SelectId = selectId;
    }

    public ulong SelectId { get; }

    public T Option { get; }

    public int Row { get; }

    public bool IsDefault { get; set; }

    public override string ToString() => Option.ToString();

    public override int GetHashCode() => Option.GetHashCode();

    public override bool Equals(object obj) => Equals(Option, obj);
}
