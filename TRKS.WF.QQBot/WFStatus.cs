using System;
using System.Globalization;
using System.Net;
using System.Text;
using Humanizer;
using Humanizer.Localisation;
using Newbe.Mahua;
using WarframeNET;

namespace TRKS.WF.QQBot
{

    public class CetusCycle
    {
        public string ID { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsDay { get; set; }
        public string TimeLeft { get; set; }
        public bool IsCetus { get; set; }
        public string ShortString { get; set; }
    }


    class WFStatus
    {
        private readonly WFChineseAPI api = new WFChineseAPI();
        public void SendCetusCycle(string group)
        {
            var cycle = api.GetCetusCycle();
            var msg = WFFormatter.ToString(cycle);
            
            Messenger.SendGroup(@"[QQ:pic=表情包\1551.jpg]" + msg, group);
        }
        
    }
}
