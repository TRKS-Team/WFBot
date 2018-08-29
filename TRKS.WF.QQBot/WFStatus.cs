using System;
using System.Globalization;
using System.Net;
using System.Text;
using Humanizer;
using Humanizer.Localisation;
using Newbe.Mahua;

namespace TRKS.WF.QQBot
{

    public class CetusCycle
    {
        public string id { get; set; }
        public DateTime expiry { get; set; }
        public bool isDay { get; set; }
        public string timeLeft { get; set; }
        public bool isCetus { get; set; }
        public string shortString { get; set; }

        public DateTime GetRealTime()
        {
            return expiry + TimeSpan.FromHours(8);
        }
    }


    class WFStatus
    {
        public void SendCetusCycle(string group)
        {
            var cycle = WebHelper.DownloadJson<CetusCycle>("https://api.warframestat.us/pc/cetusCycle");

            var time = (cycle.GetRealTime() - DateTime.Now).Humanize(int.MaxValue, CultureInfo.GetCultureInfo("zh-CN"), TimeUnit.Hour, TimeUnit.Millisecond, " ");
            var status = cycle.isDay ? "白天" : "夜晚";
            var nexttime = cycle.isDay ? "夜晚" : "白天";

            var sb = new StringBuilder();
            sb.AppendLine($"现在平原的时间是: {status}");
            sb.AppendLine($"距离 {nexttime} 还有 {time}");
            sb.Append($"在 {cycle.GetRealTime()}");
            
            sb.ToString().SendToGroup(group);
        }
    }
}
