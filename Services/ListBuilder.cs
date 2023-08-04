using XBOT.DataBase.Models.Roles_data;

namespace XBOT.Services.Configuration
{
    public class ListBuilder
    {
        private ComponentEventService _componentEventService;

        public ListBuilder(ComponentEventService componentEventService)
        {
            _componentEventService = componentEventService;
        }

        public async Task<(ulong MessageId, int SelectedIndex)> ListButtonSliderBuilder(IEnumerable<object> item, EmbedBuilder emb, string CommandName, SocketCommandContext Context, int countslots = 10, bool Selected = false)
        {
            ulong MessageId = 0;
            if (Selected)
                countslots = 3;

            int page = 1;
            var Skipped = item.Skip((page - 1) * countslots).Take(countslots);
            Lists(Skipped, CommandName,ref emb);

            if(item.Count() <= countslots && !Selected)
            {
                await Context.Channel.SendMessageAsync("", false, emb.Build());
                return (0,0);
            }



            var ButtonList = new Dictionary<string, ButtonBuilder>();
            var buttonLabels = new string[] { "Left", "Right", "1", "2", "3", "4", "5" };
            foreach (var label in buttonLabels)
            {
                ButtonList.Add(label, new ButtonBuilder
                {
                    Label = label,
                    CustomId = Guid.NewGuid().ToString(),
                    Style = ButtonStyle.Secondary
                });
            }

            var elementList = new HashSet<string>();
            var Comp = ComponentFormated(false);


            ComponentBuilder ComponentFormated(bool newButton)
            {
                var comp = new ComponentBuilder();
                if (!newButton)
                {
                    Skipped = item.Skip((page - 1) * countslots).Take(countslots);
                    Lists(Skipped, CommandName, ref emb);
                }

                var MaxPage = (item.Count() + countslots - 1) / countslots;
                emb.WithFooter($"Страница {page}/{MaxPage}");



                if (page != MaxPage)
                    ButtonRegister("Left", newButton);

                if(Selected)
                {
                    var MinValue = Math.Min(countslots, Skipped.Count()); // Узнаем минимальное значение из двух.
                    for (int i = 1; i <= MinValue; i++)
                    {
                        ButtonRegister($"{i}", newButton);
                    }
                }

                if (page > 1)
                    ButtonRegister("Right", newButton);



                void ButtonRegister(string name, bool newButton = false)
                {
                    var Button = ButtonList[name];
                    comp.WithButton(Button);

                    if (!newButton)
                        elementList.Add(Button.CustomId);
                }

                return comp;
            }


            ComponentEvent<SocketMessageComponent> userInteraction = new ComponentEvent<SocketMessageComponent>();
            _componentEventService.AddInteraction(elementList, userInteraction);

            var mes = await Context.Channel.SendMessageAsync("", false, emb.Build(), components: Comp.Build());
            var SelectedValueIndex = 0;
            while (true)
            {
                var selectedOption = await userInteraction.WaitForInteraction();

                if (selectedOption == null) 
                    break;

                var valueTip = selectedOption.Data.CustomId;
                ButtonList.TryGetValue(valueTip, out var ButtonTip);

                if(ButtonTip.Label == "Left" || ButtonTip.Label == "Right")
                {
                    if (ButtonTip.Label == "Left")
                        page--;
                    else
                        page++;

                    Comp = ComponentFormated(true);

                    await mes.ModifyAsync(x =>
                    {
                        x.Embed = emb.Build();
                        x.Components = Comp.Build();
                    });
                }
                else
                {
                    int SelectedNumber = Convert.ToInt32(ButtonTip.Label);
                    SelectedValueIndex = (page - 1) * countslots + SelectedNumber;
                    break;
                }

            }
            await mes.ModifyAsync(x => x.Components = new ComponentBuilder().Build());
            foreach (var element in elementList)
            {
                _componentEventService.RemoveInteraction(element);
            }
            
            return (MessageId, SelectedValueIndex);
        }


        private static void Lists(IEnumerable<object> items, string CommandName,ref EmbedBuilder emb)
        {
            if (CommandName == "buyrole")
            {
                emb.WithDescription("");
                int i = 0;
                foreach (var Item in items.OfType<Roles_Buy>())
                {
                    i++;
                    emb.Description += $"{i}.<@&{Item.RoleId}> - {Item.Price} coins\n";
                }
            }
            else if (CommandName == "emojigiftshop")
            {
                emb.WithDescription("");
                int i = 0;
                foreach (var Item in items.OfType<EmojiGift>())
                {
                    i++;
                    emb.Description += $"{Item.Name} - {Item.PriceTrade} price [{Item.Emoji.Factor}]\n";
                }
            }
        }
    }
}
