using System;
using Sisters.WudiLib.WebSocket;
using WFBot.Connector;
using WFBot.Features.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace OneBotConnector
{
    public class Connector : WFBotConnectorBase
    {
        CqHttpWebSocketApiClient client;
        CqHttpWebSocketEvent wsevent;
        public override void Init()
        {
            var config = OneBotConfig.Instance;
            if (config.isfirsttime) 
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

            }

            var url = $"ws://{config.host}:{config.port}";
            client = new CqHttpWebSocketApiClient($"{url}/api", config.accesstoken);
            wsevent = new CqHttpWebSocketEvent($"{url}/event", config.accesstoken);
            wsevent.ApiClient = client;
            wsevent.MessageEvent += (api, message) =>
            {
                switch (message.MessageType)
                {
                    case "private":
                        ReportFriendMessage(message.Source.UserId, message.Content.Raw);
                        break;
                    case "group":
                        ReportGroupMessage(((GroupEndpoint)message.Endpoint).GroupId, message.Source.UserId,
                            message.Content.Raw);
                        break;
                }
            };


        }

        public override void SendGroupMessage(GroupID id, string message)
        {
            client.SendGroupMessageAsync(id, message).Wait();
        }

        public override void SendPrivateMessage(UserID id, string message)
        {
            client.SendPrivateMessageAsync(id, message).Wait();
        }
    }
}
