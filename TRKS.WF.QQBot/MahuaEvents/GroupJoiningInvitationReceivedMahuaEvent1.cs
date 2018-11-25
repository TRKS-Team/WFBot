using Newbe.Mahua.MahuaEvents;
using System;
using Newbe.Mahua;

namespace TRKS.WF.QQBot.MahuaEvents
{
    /// <summary>
    /// 入群邀请接收事件
    /// </summary>
    public class GroupJoiningInvitationReceivedMahuaEvent1
        : IGroupJoiningInvitationReceivedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;

        public GroupJoiningInvitationReceivedMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessJoinGroupRequest(GroupJoiningRequestReceivedContext context)
        {
            if (Config.Instance.AcceptInvitation)
            {
                _mahuaApi.AcceptGroupJoiningInvitation(context.GroupJoiningRequestId, context.ToGroup, context.FromQq);
                Messenger.SendPrivate(Config.Instance.QQ, $"接受了来自{context.FromQq}邀请加入群{context.ToGroup}的邀请.");
            }
        }
    }
}
