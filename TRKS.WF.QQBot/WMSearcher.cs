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
        private WFTranslator translator => WFResource.WFTranslator;
        private bool isWFA = !string.IsNullOrEmpty(Config.Instance.ClientId) &&
                             !string.IsNullOrEmpty(Config.Instance.ClientSecret);

        private string platform => Config.Instance.Platform.ToString();
        public WMInfo GetWMInfo(string searchword)
        {
            var header = new WebHeaderCollection();
            var platform = Config.Instance.Platform.GetSymbols().First();
            if (Config.Instance.Platform == Platform.NS)
            {
                platform = "switch";
            }
            header.Add("platform", platform);
            var info = WebHelper.DownloadJson<WMInfo>($"https://api.warframe.market/v1/items/{searchword}/orders?include=item");
            return info;
        }

        public WMInfoEx GetWMINfoEx(string searchword)
        {
            var header = new WebHeaderCollection();
            header.Add("Authorization", $"Bearer {Config.Instance.AcessToken}");
            var platform = Config.Instance.Platform.GetSymbols().First();
            if (Config.Instance.Platform == Platform.NS)
            {
                platform = "ns";
            }
            var info = WebHelper.DownloadJson<WMInfoEx>($"https://api.richasy.cn/wfa/basic/{platform}/wm/{searchword}", header);
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

        public void SendWMInfo(string item, GroupNumber group, bool quickReply)
        {
            // 下面 你将要 看到的 是 本项目 最大的  粪山
            var words = new List<string>{"prime", "p", "甲"};
            var heads = new List<string> { "头部神经光", "头部神经", "头部神", "头部", "头"};
            foreach (var word in words)
            {
                foreach (var head in heads)
                {
                    if (!item.Contains("头部神经光元"))
                    {
                        if (item.Contains(word + head))
                        {
                            item = item.Replace(word + head, word + "头部神经光元");
                            break;
                        }
                    }
                }
            }
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
                            sb.AppendLine($"物品 {item} 不存在或格式错误.");
                            sb.AppendLine($"请问这下面有没有你要找的物品呢?（可尝试复制下面的名称来进行搜索)");
                            foreach (var similarresult in similarlist)
                            {
                                sb.AppendLine($"    {similarresult}");
                            }

                            sb.AppendLine("注: 这个命令是用来查询 WarframeMarket 上面的物品的, 不是其他什么东西.");
                            Messenger.SendGroup(group, sb.ToString().Trim());
                            return;
                        }
                    }
                }
            }

            var msg = string.Empty;
            Messenger.SendGroup(group, "好嘞, 等着, 着啥急啊, 这不帮你查呢.");

            var failed = false;
            if (Config.Instance.IsThirdPartyWM)
            {
                try
                {
                    if (isWFA)
                    {
                        var infoEx = GetWMINfoEx(searchword);
                        if (infoEx.orders.Any())
                        {
                            OrderWMInfoEx(infoEx);
                            translator.TranslateWMOrderEx(infoEx, searchword);
                            msg = WFFormatter.ToString(infoEx, quickReply);
                        }
                        else
                        {
                            msg = $"抱歉, WarframeMarket 上目前还没有售卖 {item} 的用户";
                        }
                    }
                    else
                    {
                        msg = "很抱歉, 本机器人没有 WFA 授权, 无法使用第三方 WM, 这很可能是由于错误设置导致的. 请联系机器人负责人.";
                    }
                }
                catch (Exception)
                {
                    Messenger.SendGroup(group, "很抱歉, 在使用第三方 API 时遇到了网络问题. 正在为您转官方 API.");
                    failed = true;
                }
            }

            if (!Config.Instance.IsThirdPartyWM || failed)
            {
                var info = GetWMInfo(searchword);
                if (info.payload.orders.Any())
                {
                    OrderWMInfo(info);
                    translator.TranslateWMOrder(info, searchword);
                    msg = WFFormatter.ToString(info, quickReply);
                }
                else
                {
                    msg = $"抱歉, WarframeMarket 上目前还没有售卖 {item} 的用户";
                }

            }

            if (!quickReply)
            {
                msg = $"{msg}\n\n如果你需要快捷回复, 请使用指令 <查询 {item} -QR>";
            }

            Messenger.SendGroup(group, msg.AddPlatformInfo());
        }
    }
}
