﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WFBot.Orichalt
{
    public static class OrichaltContextExtension
    {
        public static string GetInfo(this OrichaltContext o)
        {
            switch (o.Platform)
            {
                case MessagePlatform.OneBot:
                    var onebotcontext = MiguelNetwork.OrichaltContextManager.GetOneBotContext(o);
                    return 
                        $"平台[OneBot] 群[{onebotcontext.Group}] 用户[{onebotcontext.SenderID}] 内容[{onebotcontext.RawMessage}]";
                case MessagePlatform.Kook:
                    var kookcontext = MiguelNetwork.OrichaltContextManager.GetKookContext(o);
                    return
                        $"平台[Kook] 服务器[{kookcontext.Guild.Name}({kookcontext.Guild.Id})] 用户[{kookcontext.Author.Username}#{kookcontext.Author.IdentifyNumber}] 内容[{kookcontext.CleanContent}]";
                case MessagePlatform.QQChannel:
                    throw new NotImplementedException();
                case MessagePlatform.MiraiHTTP:
                    var miraihttpcontext = MiguelNetwork.OrichaltContextManager.GetMiraiHTTPContext(o);
                    return
                        $"平台[MiraiHTTP] 群[{miraihttpcontext.Group}] 用户[{miraihttpcontext.SenderID}] 内容[{miraihttpcontext.RawMessage}]";
                case MessagePlatform.MiraiHTTPV1:
                    var miraihttpcontext1 = MiguelNetwork.OrichaltContextManager.GetMiraiHTTPV1Context(o);
                    return
                        $"平台[MiraiHTTPV1] 群[{miraihttpcontext1.Group}] 用户[{miraihttpcontext1.SenderID}] 内容[{miraihttpcontext1.RawMessage}]";
            }

            return "";
        }
        public static string GetGroupIdentifier(this OrichaltContext o)
        {
            switch (o.Platform)
            {
                case MessagePlatform.OneBot:
                    var onebotcontext = MiguelNetwork.OrichaltContextManager.GetOneBotContext(o);
                    return $"QQ:{onebotcontext.Group}";
                case MessagePlatform.Kook:
                    var kookcontext = MiguelNetwork.OrichaltContextManager.GetKookContext(o);
                    return $"频道ID:{kookcontext.Channel.Id}";
                case MessagePlatform.QQChannel:
                    throw new NotImplementedException();
                case MessagePlatform.MiraiHTTP:
                    var miraihttpcontext = MiguelNetwork.OrichaltContextManager.GetMiraiHTTPContext(o);
                    return $"QQ:{miraihttpcontext.Group}";

                case MessagePlatform.MiraiHTTPV1:
                    var miraihttpcontext1 = MiguelNetwork.OrichaltContextManager.GetMiraiHTTPV1Context(o);
                    return $"QQ:{miraihttpcontext1.Group}";
            }

            return "";
        }
        public static string GetSenderIdentifier(this OrichaltContext o)
        {
            switch (o.Platform)
            {
                case MessagePlatform.OneBot:
                    var onebotcontext = MiguelNetwork.OrichaltContextManager.GetOneBotContext(o);
                    return $"QQ:{onebotcontext.SenderID}";
                case MessagePlatform.Kook:
                    var kookContext = MiguelNetwork.OrichaltContextManager.GetKookContext(o);
                    return $"用户:{kookContext.Author}";
                case MessagePlatform.QQChannel:
                    throw new NotImplementedException();
                case MessagePlatform.MiraiHTTP:
                    var miraihttpcontext = MiguelNetwork.OrichaltContextManager.GetMiraiHTTPContext(o);
                    return $"QQ:{miraihttpcontext.SenderID}";

                case MessagePlatform.MiraiHTTPV1:
                    var miraihttpcontext1 = MiguelNetwork.OrichaltContextManager.GetMiraiHTTPV1Context(o);
                    return $"QQ:{miraihttpcontext1.SenderID}";
            }

            return "";
        }
    }
}
