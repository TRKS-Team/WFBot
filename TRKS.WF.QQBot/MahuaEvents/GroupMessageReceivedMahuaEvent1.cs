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
        private static readonly WFNotificationHandler _wFAlert = new WFNotificationHandler();
        private static readonly WFStatus _wFStatus = new WFStatus();
        private static readonly WMSearcher _wmSearcher = new WMSearcher();

        public GroupMessageReceivedMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessGroupMessage(GroupMessageReceivedContext context)
        {
            try
            {
                var message = context.Message;
                if (message.StartsWith("/"))
                {
                    var command = message.Substring(1);
                    var ostrons = new List<string>() {"赏金", "平原赏金", "希图斯赏金", "希图斯", "地球赏金"};
                    var fissures = new List<string>() {"裂隙", "裂缝", "查询裂缝", "查询裂隙"};
                    if (ostrons.Where(ostron => command.StartsWith(ostron)).Any())
                    {
                        var index = command.Substring(ostrons.Where(ostron => command.StartsWith(ostron)).First().Length);
                        if (index.IsNumber())
                        {
                            if (Int32.Parse(index) <= 5 && 0 < Int32.Parse(index))
                            {
                                _wFStatus.SendOstronsMissions(context.FromGroup, Int32.Parse(index));
                            }
                            else
                            {
                                _wFStatus.SendOstronsMissions(context.FromGroup, 1);
                            }

                        }
                        else
                        {
                            _wFStatus.SendOstronsMissions(context.FromGroup, 1);
                        }
                    }

                    if (fissures.Where(fissure => command.StartsWith(fissure)).Any())
                    {
                        var words = command.Split(' ').ToList();
                        if (words.Count >= 2)
                        {
                            words.RemoveAt(0);
                            _wFStatus.SendFissures(context.FromGroup, words);
                        }
                        else
                        {
                            Messenger.SendGroup(context.FromGroup, $"需要参数,列如: /查询裂隙 古纪(第一个为主条件) 歼灭(之后可添加多个小条件).");
                        }

                    }
                    if (command.StartsWith("查询"))
                    {
                        var item = command.Substring(3).Replace(" ", "").ToLower();
                        _wmSearcher.SendWMInfo(item, context.FromGroup);
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
                            _wFStatus.SendCetusCycle(context.FromGroup);
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
                            break;
                    }
                }


            }
            catch (Exception e)
            {
                _mahuaApi.SendPrivateMessage("1141946313", e.ToString());
            }

        }
    }
}
