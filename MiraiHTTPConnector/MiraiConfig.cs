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
        public string Host;
        public short Port;
        public string AuthKey;

        public long BotQQ = default;
    }
}
