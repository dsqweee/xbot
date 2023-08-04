namespace XBOT.DataBase.Models
{
    public class EmojiGift_emojiadded
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public double Factor { get; set; } // 0.05 - rare | 0.1 - legendary | 0.5 - 
        public virtual ICollection<EmojiGift> emojiGifts { get; set; }
    }
}
