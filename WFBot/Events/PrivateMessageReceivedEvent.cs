using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GammaLibrary.Extensions;
using TextCommandCore;
using WFBot.Features.Utils;
using Number = System.Numerics.BigInteger;
using static WFBot.Features.Utils.Messenger;

namespace WFBot.Events
{
    public class PrivateMessageReceivedEvent
    {


        public void ProcessPrivateMessage(UserID senderId, string message)
        {

            new PrivateMessageHandler(senderId, message).ProcessCommandInput();
            // SendPrivate(context.FromQq, "您群号真牛逼."); // 看一次笑一次 8 时代变了

        }
    }


    public partial class PrivateMessageHandler
    {
        private List<GroupInfo> GetGroups()
        {
            return null; // TODO
        }

        [Matchers("加载配置", "UpdateConfig")]
        [RequireAdmin]
        string UpdateConfig()
        {
            Config.Update();
            return "搞完啦, 希望你没弄丢什么数据?";
        }

        [Matchers("保存配置", "SaveConfig")]
        [RequireAdmin]
        string SaveConfig()
        {
            Config.Save();
            return "搞完啦, 希望你没弄丢什么数据?";
        }
        [Matchers("所有群")]
        [RequireAdmin]
        string DumpGroups()
        {
            SendPrivate(Sender, "正在dump所有群...请稍候...结果将存储于机器人根目录...");

            Directory.CreateDirectory("所有群");
            File.WriteAllLines(@"所有群\所有群.txt", GetGroups().Select(info => $"{info.ID} {info.Name}"));

            return "搞完了, 去机器人根目录看结果.";
        }
        [Matchers("自动更新")]
        [RequireAdmin, RequireCode]
        void RunAutoUpdate()
        {
            //AutoUpdateRR.Execute();
        }

        [Matchers("没有开启通知的群")]
        [RequireAdmin, RequireCode]
        string DisabledGroups()
        {
            var groups = GetGroups();
            var gs = groups.Where(g => !groups.Contains(g)).Select(g => $"{g.Name}-{g.Name}");

            return gs.Connect("\r\n");
        }

        [RequireContainsCode]
        [SaveConfig]
        [Matchers("添加群")]
        string AddGroup(string code, Number group)
        {
            var groupStr = group.ToString();
            if (Config.Instance.WFGroupList.Contains(groupStr)) return "群号已经存在";
            Config.Instance.WFGroupList.Add(groupStr);

            SendGroup(groupStr, $"{Sender}已经在私聊启用了此群的新任务通知功能.");
            SendDebugInfo($"{groupStr}启用了通知功能.");

            return "完事.";
        }

        [RequireContainsCode]
        [SaveConfig]
        [Matchers("删除群")]
        string DeleteGroup(string code, Number group)
        {
            var groupStr = group.ToString();
            Config.Instance.WFGroupList.Remove(groupStr);

            SendGroup(groupStr, $"{Sender}已经在私聊禁用了此群的新任务通知功能.");
            SendDebugInfo($"{groupStr}禁用了通知功能.");

            return "完事.";
        }
        [RequireAdmin]
        [Matchers("更新翻译API")]
        string UpdateTranslateApi()
        {
            if (!WFResource.UpdateLexion())
                return "翻译API更新失败, 可能是请求次数过多, 请查看 FAQ 来了解如何解决这个问题.";
            return null;
        }

        [RequireAdmin]
        [Matchers("超级广播")]
        void Broadcast(string content)
        {
            //Messenger.SuperBroadcast(content);
        }
    }

    public partial class PrivateMessageHandler : ICommandHandler<PrivateMessageHandler>, ISender
    {
        public Action<TargetID, Message> MessageSender { get; } = (id, msg) => SendPrivate(id.ID, msg);
        public Action<Message> ErrorMessageSender { get; } = msg => SendDebugInfo(msg);
        public UserID Sender { get; }
        public string Message { get; }

        string ICommandHandler<PrivateMessageHandler>.Sender => Sender;

        public PrivateMessageHandler(UserID sender, string message)
        {
            Sender = sender;
            Message = message;
        }
    }
}
