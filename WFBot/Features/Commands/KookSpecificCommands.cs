using System.Globalization;
using Humanizer;
using WFBot.Features.Utils;
using WFBot.Orichalt;
using WFBot.TextCommandCore;

namespace WFBot.Features.Commands
{
    public partial class CommandsHandler
    {
        /*[SkipBotChannelCheck]
        [SkipValidationCheck]
        [KookOnly]
        [Matchers("设置通知频道", "设置通知")]
        void AddNotifyChannel()
        {
            MiguelNetwork.KookCore.SetKookNotifyChannel(O);
        }

        [SkipBotChannelCheck]
        [SkipValidationCheck]
        [KookOnly]
        [Matchers("取消通知频道", "取消通知")]
        void RemoveNotifyChannel()
        {
            MiguelNetwork.KookCore.RemoveKookNotifyChannel(O);
        }

        [SkipBotChannelCheck]
        [SkipValidationCheck]
        [KookOnly]
        [Matchers("设置机器人频道", "设置机器人")]
        void AddBotChannel()
        {
            MiguelNetwork.KookCore.SetKookBotChannel(O);
        }

        [SkipBotChannelCheck]
        [SkipValidationCheck]
        [KookOnly]
        [Matchers("取消机器人频道", "取消机器人")]
        void RemoveBotChannel()
        {
            MiguelNetwork.KookCore.RemoveKookBotChannel(O);
        }

        [SkipBotChannelCheck]
        [SkipValidationCheck]
        [KookOnly]
        [Matchers("兑换")]
        [CombineParams]
        string Redeem(string code)
        {
            return MiguelNetwork.KookVerifyServer.RedeemCode(O, code).Result ? "兑换成功, 请输入 /有效期 查询有效期" : "兑换失败, 请检查兑换码有效性.";
        }
        [SkipBotChannelCheck]
        [SkipValidationCheck]
        [KookOnly]
        [Matchers("有效期")]
        string ExpireTime()
        {
            return $"本服务器的机器人有效期为: {MiguelNetwork.KookVerifyServer.GetGuildValidationTime(O).Result.ToString(CultureInfo.CurrentCulture)}\n状态: {(MiguelNetwork.KookVerifyServer.VerifyGuild(O).Result ? "未过期": "已过期")}";
        }

        [SkipBotChannelCheck]
        [SkipValidationCheck]
        [KookOnly]
        [Matchers("试用")]
        string Trial()
        {
            return MiguelNetwork.KookVerifyServer.StartGuildTrial(O).Result ? "成功激活三天试用, 请输入 /有效期 查询有效期." : "激活试用失败, 此服务器已经试用过机器人.";
        }*/
    }
}
