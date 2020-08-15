using WFBot.Features.Utils;

namespace WFBot.Events
{
    public interface ISender
    {
        UserID Sender { get; }
    }
}