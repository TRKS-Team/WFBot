using Newbe.Mahua.MahuaEvents;
using System;
using Newbe.Mahua;

namespace TRKS.WF.QQBot.MahuaEvents
{
    /// <summary>
    /// 入群申请接收事件
    /// </summary>
    public class GroupJoiningRequestReceivedMahuaEvent1
        : IGroupJoiningRequestReceivedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;

        public GroupJoiningRequestReceivedMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessJoinGroupRequest(GroupJoiningRequestReceivedContext context)
        {
            if (Config.Instance.AcceptJoiningRequest)
            {
                _mahuaApi.AcceptGroupJoiningRequest(context.GroupJoiningRequestId, context.ToGroup, context.FromQq);
                Messenger.SendGroup(Config.Instance.QQ, $"{context.FromQq}加入了群{context.ToGroup}.");
            }
        }
    }
}
