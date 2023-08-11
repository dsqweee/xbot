namespace XBOT.Services;

public class EmojiGiftService
{
    private readonly Db _db;

    public EmojiGiftService(Db db)
    {
        _db = db;
    }

    public (string name, double factory) GenerateEmoji()
    {
            var emojiAllowed = _db.EmojiGift_emojiadded.ToList();

            var Random = new Random();
            var ElementIndex = Random.Next(emojiAllowed.Count());
            double factory = Random.NextDouble();

            var emojigift = emojiAllowed.ElementAt(ElementIndex);
            return (emojigift.Name, factory);
    }

    public void UserSetEmoji(ulong userId)
    {
        //using (Db _db = new ())
        //{
        //    var user = db.User.Include(x=>x.EmojiGift).FirstOrDefault(x=>x.Id == userId);


        //}
    }

}
