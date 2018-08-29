using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TRKS.WF.QQBot
{
    public static class WebHelper
    {
        private static WebClient webClient = new WebClient();

        public static T DownloadJson<T>(string url)
        {
            return webClient.DownloadString(url).JsonDeserialize<T>();
        }
    }
}
