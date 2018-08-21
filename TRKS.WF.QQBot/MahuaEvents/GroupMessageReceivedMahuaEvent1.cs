using Newbe.Mahua.MahuaEvents;
using System;
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

        private readonly WFAlertHandler _wFAlert = new WFAlertHandler();

        public GroupMessageReceivedMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessGroupMessage(GroupMessageReceivedContext context)
        {
            if (context.Message.StartsWith("/"))
            {
                if (context.Message.Contains("警报"))
                {
                    _wFAlert.SendAllAlerts(context.FromGroup);
                }
            }
        }
    }
}
