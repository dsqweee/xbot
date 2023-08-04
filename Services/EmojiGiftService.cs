using Microsoft.EntityFrameworkCore;

namespace XBOT.Services
{
    public class EmojiGiftService
    {
        public (string name, double factory) GenerateEmoji()
        {
            using (var _db = new db())
            {
                var emojiAllowed = _db.EmojiGift_emojiadded.ToList();

                var Random = new Random();
                var ElementIndex = Random.Next(emojiAllowed.Count());
                double factory = Random.NextDouble();

                var emojigift = emojiAllowed.ElementAt(ElementIndex);
                return (emojigift.Name, factory);
            }
        }

        public void UserSetEmoji(ulong userId)
        {
            //using (var _db = new db())
            //{
            //    var user = _db.User.Include(x=>x.EmojiGift).FirstOrDefault(x=>x.Id == userId);


            //}
        }

    }
}
