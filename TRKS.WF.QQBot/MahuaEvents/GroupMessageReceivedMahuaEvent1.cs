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
        private readonly WFNotificationHandler _wFAlert = new WFNotificationHandler();
        private readonly WFStatus _wFStatus = new WFStatus();

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
                    switch (command)
                    {
                        case "警报":
                            _wFAlert.SendAllAlerts(context.FromGroup);
                            break;
                        case "平原":
                            _wFStatus.SendCetusCycle(context.FromGroup);
                            break;
                        case "入侵":
                            _wFAlert.SendAllInvasions(context.FromGroup);
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
