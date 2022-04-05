using System;
using Sisters.WudiLib.Posts;
using Sisters.WudiLib.WebSocket;
using WFBot.Features.Events;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Orichalt.OrichaltConnectors
{
    public class OneBotContext : PlatformContextBase
    {
        public OneBotContext(string rawMessage, GroupID @group, UserID senderId, MessageType type, DateTimeOffset time)
        {
            RawMessage = rawMessage;
            Group = @group;
            SenderID = senderId;
            Type = type;
            Time = time;
        }

        public string RawMessage { get; set; }
        public GroupID Group { get; set; }
        public UserID SenderID { get; set; }
        public MessageType Type { get; set; }
        public DateTimeOffset Time { get; set; }
    }

    public enum MessageType
    {
        Private,
        Group
    }
    [Configuration("OneBotConfig")]
    class OneBotConfig : Configuration<OneBotConfig>
    {
        public string host = "127.0.0.1";
        public string port = "6700";
        public string accesstoken = "";
        // public bool isfirsttime = true;
    }
    public partial class Connectors
    {
        public CqHttpWebSocketApiClient OneBotClient;
        private CqHttpWebSocketEvent wsevent;
        public event EventHandler<OneBotContext> OneBotMessageReceived;
        public void InitOneBot()
        {
            var config = OneBotConfig.Instance;
            /*if (config.isfirsttime) 
            {
                Console.WriteLine("看起来你是第一次使用OneBotConnector, 配置文件位于OneBotConfig.json, 是否在控制台快速修改?(Y/N)");
                config.isfirsttime = false;
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    while (true)
                    {
                        Console.WriteLine("请输入OneBot协议的正向WebSocket地址:");
                        var host = Console.ReadLine();
                        Console.WriteLine("请输入OneBot协议的正向WebSocket端口:");
                        var port = Console.ReadLine();
                        Console.WriteLine("请输入OneBot协议的正向WebSocket的AccessToken:");
                        var token = Console.ReadLine();
                        Console.WriteLine("是否重新输入?(Y/N)");
                        if (Console.ReadLine()?.ToLower() == "y")
                        {
                            continue;
                        }

                        config.host = host;
                        config.port = port;
                        config.accesstoken = token;
                        OneBotConfig.Save();
                        Console.WriteLine("已保存, 你可以在OneBotConfig.json中随时修改这些参数.");
                        break;
                    }
                }

            }*/

            var url = $"ws://{config.host}:{config.port}";
            OneBotClient = new CqHttpWebSocketApiClient($"{url}/api", config.accesstoken);
            wsevent = new CqHttpWebSocketEvent($"{url}/event", config.accesstoken);
            wsevent.ApiClient = OneBotClient;
            wsevent.MessageEvent += (api, message) =>
            {
                switch (message.MessageType)
                {
                    case "private":
                        OnOneBotMessage(new OneBotContext(message.RawMessage, "", message.UserId.ToString(), MessageType.Private, message.Time));
                        break;
                    case "group":
                        OnOneBotMessage(new OneBotContext(message.RawMessage,((GroupEndpoint)message.Endpoint).GroupId, message.UserId, MessageType.Group, message.Time ));
                        break;
                }
            };
            wsevent.StartListen();
        }

        protected virtual void OnOneBotMessage(OneBotContext e)
        {
            OneBotMessageReceived?.Invoke(this, e);
        }
    }
}
