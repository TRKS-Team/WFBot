using System;
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
                case MessagePlatform.Kaiheila:
                    throw new NotImplementedException();
                case MessagePlatform.QQChannel:
                    throw new NotImplementedException();
                case MessagePlatform.MiraiHTTP:
                    var miraihttpcontext = MiguelNetwork.OrichaltContextManager.GetMiraiHTTPContext(o);
                    return
                        $"平台[MiraiHTTP] 群[{miraihttpcontext.Group}] 用户[{miraihttpcontext.SenderID}] 内容[{miraihttpcontext.RawMessage}]";

                case MessagePlatform.MiraiHTTPV1:
                    var miraihttpcontext1 = MiguelNetwork.OrichaltContextManager.GetMiraiHTTPV1Context(o);
                    return
                        $"平台[MiraiHTTP] 群[{miraihttpcontext1.Group}] 用户[{miraihttpcontext1.SenderID}] 内容[{miraihttpcontext1.RawMessage}]";
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
                case MessagePlatform.Kaiheila:
                    throw new NotImplementedException();
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
