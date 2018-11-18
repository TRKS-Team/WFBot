using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Humanizer;
using Humanizer.Localisation;
using Newbe.Mahua;

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

        public void SendVoidTrader(string group)
        {
            var trader = api.GetVoidTrader();
            var msg = WFFormatter.ToString(trader);

            Messenger.SendGroup(group, msg);
        }

        public void SendFortunaMissions(string group, int index)
        {
            var missions = api.GetSyndicateMissions();
            var msg = WFFormatter.ToString(missions.Where(mission => mission.syndicate == "Solaris United").ToList()[0], index - 1) + Environment.NewLine + $"您正在查看第 {index} 个赏金任务 可使用 /[关键词][空格][任务顺序] 来查询特定任务.";
            Messenger.SendGroup(group, msg);
        }

        public void SendOstronsMissions(string group, int index)
        {
            var missions = api.GetSyndicateMissions();
            var msg = WFFormatter.ToString(missions.Where(mission => mission.syndicate == "Ostrons").ToList()[0], index - 1) +Environment.NewLine + $"您正在查看第 {index} 个赏金任务 可使用 /[关键词][空格][任务顺序] 来查询特定任务.";
            Messenger.SendGroup(group, msg);
        }
    }
}
