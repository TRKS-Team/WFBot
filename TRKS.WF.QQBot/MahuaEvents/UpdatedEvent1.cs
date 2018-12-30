using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newbe.Mahua.MahuaEvents;

namespace TRKS.WF.QQBot.MahuaEvents
{
    public class UpdatedEvent1 : IPluginHotUpgradedMahuaEvent
    {
        public void HotUpgraded(PluginHotUpgradedContext context)
        {
            if (HotUpdateInfo.PreviousVersion) return;
            Messenger.SendDebugInfo("插件热更新完成。");
        }
    }
}