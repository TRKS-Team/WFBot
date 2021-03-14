using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using TextCommandCore;
using WarframeAlertingPrime.SDK.Models.Core;
using WarframeAlertingPrime.SDK.Models.Enums;
using WarframeAlertingPrime.SDK.Models.Others;
using WFBot.Features.Resource;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Features.Common
{
    public static class StringWildcard
    {
        private static WFTranslator translator => WFResources.WFTranslator;

        private static string[] ToArray(ITuple tuple)
        {
            var array = new string[tuple.Length];
            for (var i = 0; i < tuple.Length; i++)
            {
                array[i] = (string)tuple[i]!;
            }

            return array;
        }

        private static string[] ToWordArray(this object obj) =>
            obj switch
            {
                null => Array.Empty<string>(),
                string s => new[] { s },
                ITuple tuple => ToArray(tuple),
                _ => throw new ArgumentException()
            };

        public static string TrySearch(this string source, object oldstrst = default, object newstrst = default, object suffixest = default, bool neuroptics = false)
        {
            var oldstrs = oldstrst.ToWordArray();
            var newstrs = newstrst.ToWordArray();
            var suffixes = suffixest.ToWordArray();

            var formatted = source.Format();
            for (var i = 0; i < oldstrs.Length; i++)
            {
                formatted = formatted.Replace(oldstrs[i], newstrs[i]);
            }

            formatted += suffixes.Connect();
            var result = translator.TranslateSearchWord(formatted);
            if (neuroptics)
            {
                var heads = new[] { "头部神经光", "头部神经", "头部神", "头部", "头" };
                foreach (var head in heads.Where(head => !formatted.Contains("头部神经光元")).Where(head => formatted.Contains(head)))
                {
                    formatted = formatted.Replace(head, "头部神经光元");
                    result = translator.TranslateSearchWord(formatted);
                    break;
                }
            }
            return formatted == result ? source : result;
        }

    }
    public class WMInfoEx
    {
        public WarframeAlertingPrime.SDK.Models.WarframeMarket.OrderQueryResult orders { get; set; }
        public Sale sale { get; set; }
    }
    public class WMSearcher
    {
        private static WFTranslator translator => WFResources.WFTranslator;
        private WFApi api => WFResources.WFTranslateData;
        private Client wfaClient => WFResources.WFAApi.WfaClient;
        private bool isWFA => WFResources.WFAApi.isWFA;

        private string platform => Config.Instance.Platform.ToString();
        public async Task<WMInfo> GetWMInfo(string searchword)
        {
            var platform = Config.Instance.Platform.GetSymbols().First();
            if (Config.Instance.Platform == Platform.NS)
            {
                platform = "switch";
            }
            var header = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("platform", platform) };

            var info = await WebHelper.DownloadJsonAsync<WMInfo>($"https://api.warframe.market/v1/items/{searchword}/orders?include=item", header);

            info.sale = api.Sale.First(s => s.code == searchword);
            return info;
        }

        public async Task<WMInfoEx> GetWMINfoEx(string searchword)
        {
            /*var header = new WebHeaderCollection();
            header.Add("Authorization", $"Bearer {Config.Instance.AccessToken}");
            var platform = Config.Instance.Platform.GetSymbols().First();
            if (Config.Instance.Platform == Platform.NS)
            {
                platform = "ns";
            }
            var info = WebHelper.DownloadJson<WMInfoEx>($"https://api.richasy.cn/wfa/basic/{platform}/wm/{searchword}", header);*/
            var option = new WarframeMarketOrderQueryOption
            { Code = searchword, OrderStatus = new List<WMOrderStatus> { WMOrderStatus.InGame, WMOrderStatus.Online } };
            var orders = await wfaClient.GetWarframeMarketOrdersAsync(option);
            var result = new WMInfoEx { orders = orders, sale = api.Sale.First(s => s.code == searchword) };
            return result;
        }

        public void OrderWMInfo(WMInfo info, bool isbuyer)
        {
            info.payload.orders = (isbuyer ? info.payload.orders
                .Where(order => order.order_type == "buy")
                .Where(order => order.user.status == "online" || order.user.status == "ingame")
                .OrderByDescending(order => order.platinum)
                : info.payload.orders
                .Where(order => order.order_type == "sell")
                .Where(order => order.user.status == "online" || order.user.status == "ingame")
                .OrderBy(order => order.platinum))
                .Take(Config.Instance.WMSearchCount)
                .ToArray();

        }

        public void OrderWMInfoEx(WMInfoEx info, bool isbuyer)
        {
            info.orders.Items = (isbuyer ? info.orders.Items
                .Where(order => order.order_type == "buy")
                .OrderByDescending(order => order.platinum)
                : info.orders.Items
                .Where(order => order.order_type == "sell")
                .OrderBy(order => order.platinum))
                .Take(Config.Instance.WMSearchCount)
                .ToList();

        }

        public static bool Search(string item, out string searchword)
        {
            return item == (searchword = translator.TranslateSearchWord(item)) &&
                   item == (searchword = item.TrySearch(suffixest: ("一套"))) &&
                   item == (searchword = item.TrySearch(("总图", "p" ),  ("蓝图", "prime"))) &&
                   item == (searchword = item.TrySearch("p",  "prime", "一套")) &&
                   item == (searchword = item.TrySearch(("p", "头"), ("prime", "头部" ))) &&
                   item == (searchword = item.TrySearch("p", "prime", neuroptics: true));
        }

        public async Task<string> SendWMInfo(string item, bool quickReply, bool isbuyer)
        {
            // 详细逻辑图在我笔记本上有手稿
            // 不建议重构
            if (Search(item, out var searchword))
            {
                var sb = new StringBuilder();
                var similarlist = translator.GetSimilarItem(item.Format(), "wm");
                sb.AppendLine($"物品 {item} 不存在或格式错误.");
                if (similarlist.Any())
                {
                    sb.AppendLine($"请问这下面有没有你要找的物品呢?（可尝试复制下面的名称来进行搜索)");
                    foreach (var similarresult in similarlist)
                    {
                        sb.AppendLine($"    {similarresult}");
                    }
                }

                sb.AppendLine("注: 这个命令是用来查询 WarframeMarket 上面的物品的, 不是其他什么东西.");

                return sb.ToString().Trim().AddRemainCallCount();
            }

            var msg = string.Empty;
            if (Config.Instance.NotifyBeforeResult)
            {
                AsyncContext.SendGroupMessage("好嘞, 等着, 着啥急啊, 这不帮你查呢.");
            }

            var failed = false;
            if (Config.Instance.IsThirdPartyWM)
            {

                try
                {
                    if (isWFA)
                    {
                        var infoEx = await GetWMINfoEx(searchword);
                        if (infoEx.orders.Items.Any())
                        {
                            OrderWMInfoEx(infoEx, isbuyer);
                            translator.TranslateWMOrderEx(infoEx, searchword);
                            msg = WFFormatter.ToString(infoEx, quickReply, isbuyer);
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
                    AsyncContext.SendGroupMessage("很抱歉, 在使用第三方 API 时遇到了网络问题. 正在为您转官方 API.");
                    failed = true;
                }
            }

            if (!Config.Instance.IsThirdPartyWM || failed)
            {
                var info = GetWMInfo(searchword).Result;
                if (info.payload.orders.Any())
                {
                    OrderWMInfo(info, isbuyer);
                    translator.TranslateWMOrder(info, searchword);
                    msg = WFFormatter.ToString(info, quickReply, isbuyer);
                }
                else
                {
                    msg = $"抱歉, WarframeMarket 上目前还没有售卖 {item} 的用户";
                }

            }

            if (!quickReply)
            {
                msg = $"{msg}\n\n快捷回复请使用指令 <查询 {item} -QR>";
            }

            if (!isbuyer)
            {
                msg = $"{msg}\n\n查询买家请使用指令 <查询 {item} -B>";
            }

            return msg.AddPlatformInfo().AddRemainCallCount();
        }
    }
}
