using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using WFBot.Features.Resource;
using WFBot.Features.Utils;
using WFBot.Orichalt;
using WFBot.TextCommandCore;
using WFBot.Utils;
using Number = System.Numerics.BigInteger;
using static WFBot.Features.Utils.Messenger;

namespace WFBot.Events
{
    public class PrivateMessageReceivedEvent
    {


        public void ProcessPrivateMessage(OrichaltContext o)
        {
            AsyncContext.SetOrichaltContext(o);
            new PrivateMessageHandler(o, o.PlainMessage).ProcessCommandInput().Wait();
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
            MiguelNetwork.SendDebugInfo("正在dump所有群...请稍候...结果将存储于机器人根目录...");

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
        [RequireAdmin, RequireContainsCode]
        string DisabledGroups(string code)
        {
            var groups = GetGroups();
            var gs = groups.Where(g => !groups.Contains(g)).Select(g => $"{g.Name}-{g.Name}");

            return gs.Connect("\r\n");
        }
        
        [SaveConfig]
        [CombineEnd]
        [RequireContainsCode]
        [Matchers("添加自动回复")]
        string AddCustomReply(string code, string key, string value)
        {
            key = key.ToLowerInvariant();
            if (Config.Instance.CustomReplies.ContainsKey(key)) return "命令已经存在";
            Config.Instance.CustomReplies.Add(key, value);

            return "完事.";
        }


        [RequireContainsCode]
        [SaveConfig]
        [Matchers("删除自动回复")]
        string RemoveCustomReply(string code, string key)
        {
            key = key.ToLowerInvariant();
            if (!Config.Instance.CustomReplies.ContainsKey(key)) return "命令不存在.";
            Config.Instance.CustomReplies.Remove(key);

            return "完事.";
        }

        [Matchers("列出自动回复")]
        string ListCustomReply()
        {
            return Config.Instance.CustomReplies.Select(pair => $"{pair.Key}: {pair.Value}").Connect("\n\n");
        }


        [RequireContainsCode]
        [SaveConfig]
        [Matchers("添加群")]
        string AddGroup(string code, Number group)
        {
            var groupStr = group.ToString();
            if (Config.Instance.WFGroupList.Contains(groupStr)) return "群号已经存在";
            Config.Instance.WFGroupList.Add(groupStr);

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

            SendDebugInfo($"{groupStr}禁用了通知功能.");

            return "完事.";
        }

        [RequireAdmin]
        [Matchers("更新翻译API")]
        async Task<string> UpdateTranslateApi()
        {
            await Task.WhenAll(WFResourcesManager.WFResourceDic[nameof(WFTranslator)].Select(r => r.Reload(false)));
            return "好了.";
        }

        [RequireAdmin]
        [Matchers("超级广播")]
        void Broadcast(string content)
        {
            // Messenger.SuperBroadcast(content);
        }
    }

    public partial class PrivateMessageHandler : ICommandHandler<PrivateMessageHandler>
    {
        public Action<Message> ErrorMessageSender { get; } = msg => SendDebugInfo(msg);
        public Action<Message> MessageSender { get; } = (msg) =>             
        {
            MiguelNetwork.PrivateReply(AsyncContext.GetOrichaltContext(), msg);
        };
        public string Message { get; }
        public OrichaltContext O { get; private set; }
        public PrivateMessageHandler(OrichaltContext o, string message)
        {
            O = o;
            Message = message;
        }
    }
}
