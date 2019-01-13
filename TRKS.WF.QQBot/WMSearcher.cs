using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Settings;

namespace TRKS.WF.QQBot
{
    public class WMSearcher
    {
        private readonly WFTranslator translator = WFResource.WFTranslator;
        private bool isWFA = !string.IsNullOrEmpty(Config.Instance.ClientId) &&
                             !string.IsNullOrEmpty(Config.Instance.ClientSecret);
        public WMInfo GetWMInfo(string searchword)
        {
            var info = WebHelper.DownloadJson<WMInfo>($"https://api.warframe.market/v1/items/{searchword}/orders?include=item");
            return info;
        }

        public WMInfoEx GetWMINfoEx(string searchword)
        {
            var header = new WebHeaderCollection();
            header.Add("Authorization", $"Bearer {Config.Instance.AcessToken}");
            var info = WebHelper.DownloadJson<WMInfoEx>($"https://api.richasy.cn/wfa/basic/pc/wm/{searchword}", header);
            return info;
        }

        public void OrderWMInfo(WMInfo info)
        {
            info.payload.orders = info.payload.orders
                .Where(order => order.order_type == "sell")
                .Where(order => order.user.status == "online" || order.user.status == "ingame")
                .OrderBy(order => order.platinum)
                .Take(3)
                .ToArray();
        }

        public void OrderWMInfoEx(WMInfoEx info)
        {
            info.orders = info.orders
                .Where(order => order.order_Type == "sell")
                .Where(order => order.status == "online" || order.status == "ingame")
                .OrderBy(order => order.platinum)
                .Take(3)
                .ToArray();
        }

        public void SendWMInfo(string item, string group)
        {
            var searchword = translator.TranslateSearchWord(item);
            var formateditem = item;
            if (item == searchword)
            {
                searchword = translator.TranslateSearchWord(item + "一套");
                formateditem = item + "一套";
                if (formateditem == searchword)
                {
                    searchword = translator.TranslateSearchWord(item.Replace("p", "prime").Replace("总图", "蓝图"));
                    formateditem = item.Replace("p", "prime").Replace("总图", "蓝图");
                    if (formateditem == searchword)
                    {
                        searchword = translator.TranslateSearchWord(item.Replace("p", "prime") + "一套");
                        formateditem = item.Replace("p", "prime") + "一套";
                        if (formateditem == searchword)
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
                    }
                }
            }

            var msg = "";
            if (isWFA)
            {
                var infoEx = GetWMINfoEx(searchword);
                OrderWMInfoEx(infoEx);
                translator.TranslateWMOrderEx(infoEx, searchword);
                msg = WFFormatter.ToString(infoEx);
            }
            else
            {
                var info = GetWMInfo(searchword);
                OrderWMInfo(info);
                translator.TranslateWMOrder(info, searchword);
                msg = WFFormatter.ToString(info);
            }

            Messenger.SendGroup(group, msg);
        }
    }
}
