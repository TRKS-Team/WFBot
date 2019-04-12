using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GammaLibrary.Extensions;
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

            new PrivateMessageHandler(context.FromQq.ToHumanQQNumber(), context.Message).ProcessCommandInput();
            //                SendPrivate(context.FromQq, "您群号真牛逼."); // 看一次笑一次 6 皮笑肉不笑
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
            File.WriteAllLines(@"所有群\所有群.txt", GetGroups().Select(info => $"{info.Group} {info.Name}"));

            return "搞完了, 去机器人根目录看结果.";
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

                return gs.Connect("\r\n");
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

            SendGroup(groupStr.ToGroupNumber(), $"{Sender}已经在私聊启用了此群的新任务通知功能.");
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

            SendGroup(groupStr.ToGroupNumber(), $"{Sender}已经在私聊禁用了此群的新任务通知功能.");
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
            Messenger.SuperBroadcast(content);
        }
    }

    public partial class PrivateMessageHandler : ICommandHandler<PrivateMessageHandler>, ISender
    {
        public Action<TargetID, Message> MessageSender { get; } = (id, msg) => SendPrivate(id.ID.ToHumanQQNumber(), msg);
        public Action<Message> ErrorMessageSender { get; } = msg => SendDebugInfo(msg);
        public HumanQQNumber Sender { get; }
        public string Message { get; }

        string ICommandHandler<PrivateMessageHandler>.Sender => Sender.QQ;

        public PrivateMessageHandler(HumanQQNumber sender, string message)
        {
            Sender = sender;
            Message = message;
        }
    }
}
