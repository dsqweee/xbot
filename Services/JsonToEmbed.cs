using Newtonsoft.Json;

namespace XBOT.Services
{
    public class JsonToEmbed
    {
        public static (EmbedBuilder, string) JsonCheck(string text)
        {
            var emb = new EmbedBuilder();
            Root EB;
            try
            {
                EB = JsonConvert.DeserializeObject<Root>(text);

                if (EB.Title != null) emb.WithTitle(EB.Title);
                if (EB.Description != null) emb.WithDescription(EB.Description);
                if (EB.Author != null) emb.WithAuthor(EB.Author.Name, EB.Author.Icon_Url, EB.Author.Url);
                if (EB.Color != 0) emb.WithColor(new Color(EB.Color));
                if (EB.Footer != null) emb.WithFooter(EB.Footer.Text, EB.Footer.Icon_Url);
                if (EB.Thumbnail != null) emb.WithThumbnailUrl(EB.Thumbnail);
                if (EB.Image != null) emb.WithImageUrl(EB.Image);
                if(EB.Fields != null)
                {
                    foreach (var field in EB.Fields)
                    {
                        if (field.Value?.Length != 0 && field.Name?.Length != 0)
                            emb.AddField(field.Name, field.Value, field.InLine);
                    }
                }
                
            }
            catch (Exception)
            {
                return (null, null);
            }

            return (emb, EB.PlainText);
        }

        private sealed class Root
        {
            public string PlainText { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public Author Author { get; set; }
            public uint Color { get; set; }
            public Footer Footer { get; set; }
            public string Thumbnail { get; set; }
            public string Image { get; set; }
            public List<Field> Fields { get; set; }
        }
        private sealed class Author
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string Icon_Url { get; set; }
        }
        private sealed class Footer
        {
            public string Text { get; set; }
            public string Icon_Url { get; set; }
        }
        private sealed class Field
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public bool InLine { get; set; }
        }
    }
}
