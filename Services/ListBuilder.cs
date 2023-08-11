//using System.Linq;
//using XBOT.DataBase.Models.Roles_data;

//namespace XBOT.Services.Configuration
//{
//    public class ListBuilder
//    {
//        private ComponentEventService _componentEventService;

//        public ListBuilder(ComponentEventService componentEventService)
//        {
//            _componentEventService = componentEventService;
//        }

//        public async Task<(ulong MessageId, int SelectedIndex)> ListButtonSliderBuilder(IEnumerable<object> item, EmbedBuilder emb, string CommandName,ISocketMessageChannel Channel, int countslots = 10, bool Selected = false)
//        {
//            ulong MessageId = 0;
//            if (Selected)
//                countslots = 3;

//            int page = 1;
//            var Skipped = item.Skip((page - 1) * countslots).Take(countslots);
//            Lists(Skipped, CommandName,ref emb);

//            if(item.Count() <= countslots && !Selected)
//            {
//                await Channel.SendMessageAsync("", false, emb.Build());
//                return (0,0);
//            }



//            var ButtonList = new Dictionary<string, ButtonBuilder>();
//            var buttonLabels = new string[] { "Left", "Right", "1", "2", "3", "4", "5" };
//            var elementList = new HashSet<string>();
//            foreach (var label in buttonLabels)
//            {
//                var Guidid = Guid.NewGuid().ToString();
//                ButtonList.Add(label, new ButtonBuilder
//                {
//                    Label = label,
//                    CustomId = Guidid,
//                    Style = ButtonStyle.Secondary
//                });
//                elementList.Add(Guidid);
//            }

            
//            var Comp = ComponentFormated(true);


//            ComponentBuilder ComponentFormated(bool newButton)
//            {
//                var comp = new ComponentBuilder();
//                if (!newButton)
//                {
//                    Skipped = item.Skip((page - 1) * countslots).Take(countslots);
//                    Lists(Skipped, CommandName, ref emb);
//                }

//                var MaxPage = (item.Count() + countslots - 1) / countslots;
//                emb.WithFooter($"Страница {page}/{MaxPage}");


//                if (page > 1)
//                    ButtonRegister("Left");
                

//                if(Selected)
//                {
//                    var MinValue = Math.Min(countslots, Skipped.Count()); // Узнаем минимальное значение из двух.
//                    for (int i = 1; i <= MinValue; i++)
//                    {
//                        ButtonRegister($"{i}");
//                    }
//                }

//                if (page != MaxPage)
//                    ButtonRegister("Right");



//                void ButtonRegister(string name)
//                {
//                    var Button = ButtonList[name];
//                    comp.WithButton(Button);

//                    //if (!elementList.Any(x=>x == Button.CustomId))
//                    //    elementList.Add(Button.CustomId);
//                }

//                return comp;
//            }


//            ComponentEvent<SocketMessageComponent> userInteraction = new ComponentEvent<SocketMessageComponent>(elementList, false);
//            _componentEventService.AddInteraction(elementList, userInteraction);

//            var mes = await Channel.SendMessageAsync("", false, emb.Build(), components: Comp.Build());
//            var SelectedValueIndex = 0;
//            while (true)
//            {
//                var selectedOption = await userInteraction.WaitForInteraction();

//                if (selectedOption == null) 
//                    break;

//                var valueTip = selectedOption.Data.CustomId;
//                ButtonBuilder ButtonTip = null;
//                foreach (var value in ButtonList)
//                {
//                    if(value.Value.CustomId == valueTip)
//                    {
//                        ButtonTip = value.Value;
//                        break;
//                    }
//                }

//                if(ButtonTip.Label == "Left" || ButtonTip.Label == "Right")
//                {
//                    if (ButtonTip.Label == "Left")
//                        page--;
//                    else
//                        page++;

//                    Comp = ComponentFormated(false);

//                    await mes.ModifyAsync(x =>
//                    {
//                        x.Embed = emb.Build();
//                        x.Components = Comp.Build();
//                    });
//                }
//                else
//                {
//                    int SelectedNumber = Convert.ToInt32(ButtonTip.Label);
//                    SelectedValueIndex = (page - 1) * countslots + SelectedNumber;
//                    break;
//                }

//            }
//            await mes.ModifyAsync(x => x.Components = new ComponentBuilder().Build());
//            foreach (var element in elementList)
//            {
//                _componentEventService.RemoveInteraction(element);
//            }
            
//            return (MessageId, SelectedValueIndex);
//        }


//        private void Lists(IEnumerable<object> items, string CommandName,ref EmbedBuilder emb)
//        {
//            emb.WithDescription("");
//            if (CommandName == "buyrole")
//            {
//                int i = 0;
//                foreach (var Item in items.OfType<Roles_Buy>())
//                {
//                    i++;
//                    emb.Description += $"{i}.<@&{Item.RoleId}> - {Item.Price} coins\n";
//                }
//            }
//            else if (CommandName == "emojigiftshop")
//            {
//                foreach (var Item in items.OfType<EmojiGift>())
//                {
//                    emb.Description += $"{Item.Name} - {Item.PriceTrade} price [{Item.Emoji.Factor}]\n";
//                }
//            }
//            else if(CommandName == "levelrole")
//            {
//                foreach (var Item in items.OfType<Roles_Level>())
//                {
//                    emb.Description += $"{Item.Level} уровень - <@&{Item.RoleId}>\n";
//                }
//            }
//        }

//    }
//}
