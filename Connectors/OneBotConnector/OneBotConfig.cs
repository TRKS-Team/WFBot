using System;
using System.Collections.Generic;
using System.Text;
using GammaLibrary;

namespace OneBotConnector
{
    [Configuration("OneBotConfig")]
    class OneBotConfig : Configuration<OneBotConfig>
    {
        public string host = "127.0.0.1";
        public string port = "6700";
        public string accesstoken = "";
        public bool isfirsttime = true;
    }
}
