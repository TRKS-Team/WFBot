using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newbe.Mahua.MahuaEvents;

namespace TRKS.WF.QQBot.MahuaEvents
{
    public class UpdatingEvent1 : IPluginHotUpgradingMahuaEvent
    {
        public void HotUpgrading(PluginHotUpgradingContext context)
        {
            Messenger.SendDebugInfo("正在进行插件热更新。");
            GroupMessageReceivedMahuaEvent1._wFAlert.timer.Stop();
            InitEvent1.timer1.Stop();
        }
    }
}
