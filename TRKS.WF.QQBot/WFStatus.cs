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

    class WFStatus
    {
        private readonly WFChineseAPI api = new WFChineseAPI();
        public void SendCetusCycle(string group)
        {
            var cycle = api.GetCetusCycle();
            var msg = WFFormatter.ToString(cycle);
            
            Messenger.SendGroup(group, msg);
        }

        public void SendSortie(string group)
        {
            var sortie = api.GetSortie();
            var msg = WFFormatter.ToString(sortie);
            
            Messenger.SendGroup(group, msg);
        }
    }
}
