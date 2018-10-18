using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRKS.WF.QQBot
{
    public class WMSearcher
    {
        private readonly WFTranslator translator = new WFTranslator();
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
                .Take(5)
                .ToArray();
        }

        public void SendWMInfo(string item, string group)
        {
            var searchword = translator.TranslateSearchWord(item);
            if (item == searchword)
            {
                Messenger.SendGroup(group, $"物品{item}不存在或格式错误,请尝试重新搜索");
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
