using GEmojiSharp;
using Pcg;

namespace XBOT.Services;

public class EmojiGiftService
{
    private readonly Db _db;

    public EmojiGiftService(Db db)
    {
        _db = db;
    }

    public string AddEmoji(string Name, double factory)
    {
        if (factory < 0 && factory > 1)
            return "Фактор не может быть меньше 0 или больше 1.";

        var EmojiAny = _db.EmojiGift_emojiadded.FirstOrDefault(x=>x.Name == Name);
        if (EmojiAny != null)
            return $"Эмодзи уже есть в системе под номером {EmojiAny.Id}";

        var isEmoji = GEmojiSharp.Emoji.Get(Name);
        if (isEmoji == GEmoji.Empty)
            return $"{Name} - не является эмодзи!";

        _db.EmojiGift_emojiadded.Add(new EmojiGift_emojiadded { Name = Name, Factor = factory});
        _db.SaveChangesAsync();
        return "Эмодзи успешно добавлен в систему";
    }

    public string DisableEmoji(string Name)
        => ActionStatusEmoji(Name, false);

    public string EnableEmoji(string Name)
        => ActionStatusEmoji(Name, true);

    private string ActionStatusEmoji(string Name, bool Enable)
    {
        var EmojiAny = _db.EmojiGift_emojiadded.FirstOrDefault(x => x.Name == Name);
        if (EmojiAny == null)
            return $"Эмодзи нет в системе под именем {Name}";

        EmojiAny.IsDisable = !Enable;
        _db.SaveChangesAsync();
        return $"Эмодзи успешно {(Enable ? "включен" : "отключен")} в системе";
    }


    public EmojiGift_emojiadded UserSetEmoji(ulong userId)
    {
        var randomDropemoji = new PcgRandom().NextDouble();

        if (randomDropemoji > 0.3) // Chance drop emoji 
            return null;

        var randomDouble = new PcgRandom().NextDouble(); // Chance drop rare emoji
        var AllEmoji = _db.EmojiGift_emojiadded.Where(x=>!x.IsDisable).ToList();

        var RndEmoji = AllEmoji
            .OrderBy(x => x.Factor)
            .OrderBy(x => Math.Abs(x.Factor - randomDouble))
            .ElementAt(0);

        _db.EmojiGift.Add(new EmojiGift { EmojiId = RndEmoji.Id,UserId = userId });
        _db.SaveChangesAsync();
        return RndEmoji;
    }

    public string SetEmojiInTrade(ulong emojiId, ulong Price)
        => ActionTradeEmoji(emojiId, Price);

    public string DelEmojiInTrade(ulong emojiId)
        => ActionTradeEmoji(emojiId, 0);

    private string ActionTradeEmoji(ulong emojiId, ulong Price)
    {
        var GetEmoji = _db.EmojiGift.FirstOrDefault(x => x.Id == emojiId);
        if (GetEmoji == null)
            return $"Эмодзи с id `{emojiId}` не найдено.";

        GetEmoji.PriceTrade = Price;
        _db.SaveChangesAsync();
        return $"Эмодзи {GetEmoji.Name}, {(Price == 0 ? "удалено с продажи." : $"выставлено на продажу за {Price} coins.")}";
    }
}
