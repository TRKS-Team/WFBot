using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFBot.Utils;

namespace MiraiHTTPConnector
{
    [Configuration("MiraiConfig")]
    public class MiraiConfig : Configuration<MiraiConfig>
    {
        public string Host = "127.0.0.1";
        public ushort Port = 8080;
        public string AuthKey = "";

        public bool AutoRevoke = false;
        public int RevokeTimeInSeconds = 60;
        public long BotQQ = default;
    }
}
