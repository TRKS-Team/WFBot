using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Settings;

namespace TRKS.WF.QQBot
{
    public class WMSearcher
    {
        private readonly WFTranslator translator = WFResource.WFTranslator;
        public WMInfo GetWMInfo(string searchword)
        {
            var info = WebHelper.DownloadJson<WMInfo>($"https://api.warframe.market/v1/items/{searchword}/orders?include=item");
            return info;
        }

        public void OrderWMInfo(WMInfo info)
        {
            info.payload.orders = info.payload.orders
                .Where(type => type.order_type == "sell")
                .Where(status => status.user.status == "online" || status.user.status == "ingame")
                .OrderBy(plat => plat.platinum)
                .Take(3)
                .ToArray();
        }

        public void SendWMInfo(string item, string group)
        {
            var searchword = translator.TranslateSearchWord(item);
            if (item == searchword)
            {
                var sb = new StringBuilder();
                var similarlist = translator.GetSimilarItem(item.Format());
                sb.AppendLine($"物品{item}不存在或格式错误.");
                sb.AppendLine($"请问这下面有没有你要找的物品呢?:");
                foreach (var similarresult in similarlist)
                {
                    sb.AppendLine($"    {similarresult}");
                }
                Messenger.SendGroup(group, sb.ToString().Trim());
                return;
            }
            var info = GetWMInfo(searchword);
            OrderWMInfo(info);
            translator.TranslateWMOrder(info, searchword);
            var msg = WFFormatter.ToString(info);
            Messenger.SendGroup(group, msg);
        }
    }
}
