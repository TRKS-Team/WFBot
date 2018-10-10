using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;

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
