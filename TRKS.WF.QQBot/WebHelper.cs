using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TRKS.WF.QQBot
{
    public static class WebHelper
    {
        static WebHelper()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private static ThreadLocal<WebClient> webClient = new ThreadLocal<WebClient>(() => new WebClient { Encoding = Encoding.UTF8 });
        public static T DownloadJson<T>(string url)
        {
            return webClient.Value.DownloadString(url).JsonDeserialize<T>();
        }
    }
}
