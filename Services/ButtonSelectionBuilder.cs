using Fergun.Interactive.Selection;
using Fergun.Interactive;

namespace XBOT.Services;

public class ButtonSelectionBuilder<T> : BaseSelectionBuilder<ButtonSelection<T>, ButtonOption<T>, ButtonSelectionBuilder<T>>
{
    // Since this selection specifically created for buttons, it makes sense to make this option the default.
    public override InputType InputType => InputType.Buttons;

    // We must override the Build method
    public override ButtonSelection<T> Build() => new(this);
}

// Custom selection where you can override the default button style/color
public class ButtonSelection<T> : BaseSelection<ButtonOption<T>>
{
    public ButtonSelection(ButtonSelectionBuilder<T> builder)
        : base(builder)
    {
    }

    // This method needs to be overriden to build our own component the way we want.
    public override ComponentBuilder GetOrAddComponents(bool disableAll, ComponentBuilder builder = null)
    {
        builder ??= new ComponentBuilder();
        foreach (var option in Options)
        {
            string label = StringConverter?.Invoke(option);
            if (label is null)
            {
                throw new InvalidOperationException($"Neither {nameof(EmoteConverter)} nor {nameof(StringConverter)} returned a valid emote or string.");
            }

     
            var button = new ButtonBuilder()
                .WithCustomId(label)
                .WithStyle(option.Style)
                .WithDisabled(disableAll)
                .WithLabel(label);

            builder.WithButton(button);
        }

        return builder;
    }
}

public record ButtonOption<T>(T Option, ButtonStyle Style); // An option with an style
