using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using ImageMagick;
using Mirai_CSharp;
using Mirai_CSharp.Extensions;
using Mirai_CSharp.Models;
using Mirai_CSharp.Plugin;
using WFBot;
using WFBot.Connector;
using WFBot.Features.Utils;

namespace MiraiHTTPConnector
{
    public class Connector : WFBotConnectorBase
    {
        private MiraiHttpSession session;

        public override void Init()
        {
            var config = MiraiConfig.Instance;
            var qq = config.BotQQ;
            var host = config.Host;
            var port = config.Port;
            var authKey = config.AuthKey;
            if (Directory.Exists("WFBotImageCaches"))
            {
                Directory.Delete("WFBotImageCaches", true);
            }
            MiraiConfig.Save();

            if (qq == default || host == default || port == default || authKey == default)
            {
                throw new InvalidOperationException("请在 MiraiConfig.json 内补全信息, 详情请查看文档.");
            }

            var options = new MiraiHttpSessionOptions(host, port, authKey); // 至少八位数
            session = new MiraiHttpSession();
            session.GroupMessageEvt += (sender, args) =>
            {
                var msg = args.Chain.GetPlain();
                ReportGroupMessage(args.Sender.Group.Id, args.Sender.Id, msg);
                return Task.FromResult(true);
            };

            session.FriendMessageEvt += (sender, args) =>
            {
                var msg = args.Chain.GetPlain();
                ReportFriendMessage(args.Sender.Id, msg);
                return Task.FromResult(true);
            };

            session.DisconnectedEvt += async (sender, exception) =>  
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Mirai 连接断开, 正在重连...");
                        await session.ConnectAsync(options, qq);
                        return true;
                    }
                    catch (Exception)
                    {
                        await Task.Delay(1000);
                    }
                }
            };
            session.ConnectAsync(options, qq).Wait();
        }

        public override void SendGroupMessage(GroupID groupID, string message)
        {
            var isCommonMessage = message.Contains("好嘞") || 
                                  message.Contains("很抱歉, 这个命令可能需要更长的时间来执行.") ||
                                  message.Contains("I want to buy");
            // 这里仅仅为临时方案
            if (MiraiConfig.Instance.RenderTextInImage && !isCommonMessage)
            {
                var imageMsg = session.UploadPictureAsync(UploadTarget.Group, RenderImage(message)).Result;
                var regex = new Regex(
                    @"(?:(?:https?|ftp|file):\/\/|www\.|ftp\.)(?:\([-A-Z0-9+&@#\/%=~_|$?!:,.]*\)|[-A-Z0-9+&@#\/%=~_|$?!:,.])*(?:\([-A-Z0-9+&@#\/%=~_|$?!:,.]*\)|[A-Z0-9+&@#\/%=~_|$])",
                    RegexOptions.Multiline | RegexOptions.IgnoreCase).Matches(message);
                if (regex.Count > 0)
                {
                    session.SendGroupMessageAsync(groupID, imageMsg, new PlainMessage(regex.Connect("\n"))).Wait();
                }
                else
                {
                    session.SendGroupMessageAsync(groupID, imageMsg).Wait();
                }
            }
            else
            {
                session.SendGroupMessageAsync(groupID, new PlainMessage(message)).Wait();
            }
        }

        static MagickImage testImage;

        static Connector()
        {
            testImage = new MagickImage(new MagickColor(42, 43, 48), 1, 1);
            testImage.Settings.TextAntiAlias = true;
            testImage.Settings.Density = new Density(72);
            testImage.Settings.FontPointsize = 36;
            testImage.Settings.Font = "msyh.ttf";
            testImage.Settings.StrokeColor = new MagickColor(255, 255, 255);
            testImage.Settings.FillColor = new MagickColor(255, 255, 255);
        }
        string RenderImage(string message)
        {
            Directory.CreateDirectory("WFBotImageCaches");
            var path = Path.Combine("WFBotImageCaches", $"{Guid.NewGuid():D}.png");
            var metric = testImage.FontTypeMetrics(message, false);
            var margin = (int)(metric.TextHeight / 10.0);
            using var image = new MagickImage(new MagickColor(42, 43, 48), (int)metric.TextWidth /*+ margin*2*/, (int)metric.TextHeight/*+margin*2*/);
            image.Settings.TextAntiAlias = true;
            image.Settings.Density = new Density(72);
            image.Settings.FontPointsize = 36;
            image.Settings.Font = "msyh.ttf";
            image.Settings.StrokeColor = new MagickColor(255, 255, 255);
            image.Settings.FillColor = new MagickColor(255, 255, 255);

            image.Annotate(message,new MagickGeometry(margin, (int)Math.Ceiling((margin+metric.TextHeight) / 2.0)), Gravity.Northwest);
            image.Write(path);
            return path;
        }

        public override void SendPrivateMessage(UserID userID, string message)
        {
            session.SendFriendMessageAsync(userID, new PlainMessage(message)).Wait();
        }
    }
}
