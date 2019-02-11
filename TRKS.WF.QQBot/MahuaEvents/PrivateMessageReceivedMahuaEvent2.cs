using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newbe.Mahua;
using Newbe.Mahua.Internals;
using Newbe.Mahua.MahuaEvents;
using Settings;
using TextCommandCore;
using Number = System.Numerics.BigInteger;
using static TRKS.WF.QQBot.Messenger;
using MahuaPlatform = Newbe.Mahua.Internals.MahuaPlatform;

namespace TRKS.WF.QQBot.MahuaEvents
{
    /// <summary>
    /// 私聊消息接收事件
    /// </summary>
    public class PrivateMessageReceivedMahuaEvent2
        : IPrivateMessageReceivedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;
        
        public PrivateMessageReceivedMahuaEvent2(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        private void DownloadImage(string filename, string path, string name)
        {
            if (MahuaPlatformValueProvider.CurrentPlatform.Value == MahuaPlatform.Cqp)
            {
                var file = File.ReadAllLines(Path.Combine(@"data\image", filename));
                foreach (var line in file)
                {
                    if (line.StartsWith("url"))
                    {
                        var url = line.Substring(4);
                        WebHelper.DownloadFile(url, path, name);
                    }
                }
            }
        }

        public void ProcessPrivateMessage(PrivateMessageReceivedContext context)
        {
            if (HotUpdateInfo.PreviousVersion) return;

            new PrivateMessageHandler(context.FromQq).ProcessCommandInput(context.FromQq, context.Message);
            //                SendPrivate(context.FromQq, "您群号真牛逼."); // 看一次笑一次 5 皮笑肉不笑
        }
    }


    public partial class PrivateMessageHandler
    {
        private List<GroupInfo> GetGroups()
        {
            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
            {
                var mahuaApi = robotSession.MahuaApi;
                var groups = mahuaApi.GetGroupsWithModel().Model.ToList();
                return groups;
            }
        }
        [Matchers("所有群")]
        [RequireAdmin]
        string DumpGroups()
        {
            SendPrivate(Sender, "正在dump所有群...请稍后...结果将存储于机器人根目录...");
            var groups = GetGroups();
            var sb = new StringBuilder();
            foreach (var info in groups)
            {
                sb.AppendLine($"{info.Group} {info.Name}");
            }

            File.WriteAllText(@"所有群\所有群.txt", sb.ToString());
            return "搞完了,去机器人根目录看结果.";
        }
        [Matchers("自动更新")]
        [RequireAdmin, RequireCode]
        void RunAutoUpdate()
        {
            AutoUpdateRR.Execute();
        }

        [Matchers("没有开启通知的群")]
        [RequireAdmin, RequireCode]
        string DisabledGroups()
        {
            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
            {
                var api = robotSession.MahuaApi;
                var groups = api.GetGroupsWithModel().Model.ToList();
                var gs = groups.Where(g => !groups.Contains(g)).Select(g => $"{g.Group}-{g.Name}");

                return string.Join("\r\n", gs);
            }
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
    }

    public partial class PrivateMessageHandler : ICommandHandlerCollection<PrivateMessageHandler>, ISender
    {
        public Action<TargetID, Message> MessageSender { get; } = (id, msg) => SendPrivate(id, msg);
        public Action<Message> ErrorMessageSender { get; } = msg => SendDebugInfo(msg);
        public string Sender { get; }

        public PrivateMessageHandler(string sender)
        {
            Sender = sender;
        }
    }
}
