using Newbe.Mahua.MahuaEvents;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using Newbe.Mahua;

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
                if (context.Message.StartsWith("/"))
                {
                    if (context.Message.Contains("警报"))
                    {
                        _wFAlert.SendAllAlerts(context.FromGroup);
                    }

                    if (context.Message.Contains("平原"))
                    {
                        _wFStatus.SendCetusCycle(context.FromGroup);
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
