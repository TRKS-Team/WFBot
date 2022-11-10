using WFBot.Features.Utils;
using WFBot.Orichalt;
using WFBot.TextCommandCore;

namespace WFBot.Features.Commands
{
    public partial class CommandsHandler
    {
        [KookOnly]
        [Matchers("设置通知频道", "设置通知")]
        void AddNotifyChannel()
        {
            MiguelNetwork.KookCore.SetKookNotifyChannel(O);
        }

        [KookOnly]
        [Matchers("取消通知频道", "取消通知")]
        void RemoveNotifyChannel()
        {
            MiguelNetwork.KookCore.RemoveKookNotifyChannel(O);
        }

        [KookOnly]
        [Matchers("设置机器人频道", "取消机器人")]
        void AddBotChannel()
        {
            MiguelNetwork.KookCore.SetKookBotChannel(O);
        }
        [KookOnly]
        [Matchers("取消机器人频道", "取消机器人")]
        void RemoveBotChannel()
        {
            MiguelNetwork.KookCore.RemoveKookBotChannel(O);
        }
    }
}
