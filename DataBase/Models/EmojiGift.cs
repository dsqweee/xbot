namespace XBOT.DataBase.Models
{
    public class EmojiGift
    {
        public ulong Id { get; set; }
        public string Name 
        { 
            get => Emoji.Name;
        }
        public ulong UserId { get; set; }
        public virtual User User { get; set; }
        public ulong EmojiId { get; set; }
        public virtual EmojiGift_emojiadded Emoji { get; set; }
        public ulong PriceTrade { get; set; }
    }
}
