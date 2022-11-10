namespace WFBot.Features.ImageRendering
{
    public class RichMessages : List<RichMessage>
    {

        public static implicit operator RichMessages(string s)
        {
            return new RichMessages{ new TextMessage{Content = s} };
        }
    }

    public abstract class RichMessage
    {
    }

    public class TextMessage : RichMessage
    {
        public string Content { get; set; }
    }

    public class ImageMessage : RichMessage
    {
        public byte[] Content { get; set; }
    }
}
