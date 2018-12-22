using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Settings;

namespace TRKS.WF.QQBot.MahuaEvents
{
    /// <summary>
    /// 群消息接收事件
    /// </summary>
    public class GroupMessageReceivedMahuaEvent1
        : IGroupMessageReceivedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;
        internal static readonly WFNotificationHandler _wFAlert = new WFNotificationHandler();
        private static readonly WFStatus _wFStatus = new WFStatus();
        private static readonly WMSearcher _wmSearcher = new WMSearcher();
        private static readonly RMSearcher _rmSearcher = new RMSearcher();

        public GroupMessageReceivedMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessGroupMessage(GroupMessageReceivedContext context)
        {
            if (HotUpdateInfo.PreviousVersion) return;

            try
            {
                var message = context.Message;
                if (message.StartsWith("/"))
                {
                    var command = message.Substring(1).ToLower();
                    var syndicates = new [] {"赏金", "平原赏金", "地球赏金", "金星赏金", "金星平原赏金", "地球平原赏金"};
                    var fissures = new [] {"裂隙", "裂缝", "虚空裂隙", "查询裂缝", "查询裂隙"};
                    if (syndicates.Any(ostron => command.StartsWith(ostron)))
                    {
                        var indexString = command.Substring(syndicates.First(ostron => command.StartsWith(ostron)).Length);
                        if (indexString.IsNumber())
                        {
                            var index = int.Parse(indexString);
                            if (index <= 5 && 0 < index)
                            {
                                _wFStatus.SendSyndicateMissions(context.FromGroup, index);
                            }
                            else
                            {
                                _wFStatus.SendSyndicateMissions(context.FromGroup, 1);
                            }

                        }
                        else
                        {
                            Messenger.SendGroup(context.FromGroup, "需要参数, 例子:/赏金 4 将会返回赏金四.");
                        }
                    }

                    if (fissures.Any(fissure => command.StartsWith(fissure)))
                    {
                        var words = command.Split(' ').ToList();
                        if (words.Count >= 2)
                        {
                            words.RemoveAt(0);
                            _wFStatus.SendFissures(context.FromGroup, words);
                        }
                        else
                        {
                            Messenger.SendGroup(context.FromGroup, $"需要参数, 列如: /查询裂隙 古纪(第一个为主条件) 歼灭(之后可添加多个小条件).");
                        }

                    }
                    if (command.StartsWith("查询"))
                    {
                        if (!command.Contains("裂隙") || !command.Contains("裂缝"))
                        {
                            var item = command.Substring(3).Replace(" ", "").ToLower();
                            _wmSearcher.SendWMInfo(item, context.FromGroup);
                        }

                    }

                    if (command.StartsWith("紫卡"))
                    {
                        var strs = command.Split(' ');
                        var weapon = strs.Last();
                         _rmSearcher.SendRiveninfos(context.FromGroup, weapon);
                        
                    }
                    switch (command)
                    {
                        case "警报":
                            _wFAlert.SendAllAlerts(context.FromGroup);
                            break;
                        case "平野":
                        case "夜灵平野":
                        case "平原":
                        case "夜灵平原":
                        case "金星平原":
                        case "奥布山谷":
                        case "金星平原温度":
                        case "平原温度":
                            _wFStatus.SendCycles(context.FromGroup);
                            break;
                        case "入侵":
                            _wFAlert.SendAllInvasions(context.FromGroup);
                            break;
                        case "突击":
                            _wFStatus.SendSortie(context.FromGroup);
                            break;
                        case "奸商":
                        case "虚空商人":
                        case "商人":
                            _wFStatus.SendVoidTrader(context.FromGroup);
                            break;
                        case "help":
                        case "帮助":
                        case "功能":
                            Messenger.SendHelpdoc(context.FromGroup);
                            break;
                    }
                }


            }
            catch (Exception e)
            {
                Messenger.SendDebugInfo(e.ToString());
            }

        }
    }
}
