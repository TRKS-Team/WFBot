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

        public GroupMessageReceivedMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessGroupMessage(GroupMessageReceivedContext context)
        {
            if (context.Message == "添加群")
            {
                if (_mahuaApi.GetGroupMemberInfo(context.FromGroup, context.FromQq).Authority ==
                    GroupMemberAuthority.Manager ||
                    _mahuaApi.GetGroupMemberInfo(context.FromGroup, context.FromQq).Authority ==
                    GroupMemberAuthority.Leader)
                {
                    Config.Instance.WFGroupList.Add(context.FromGroup);
                    _mahuaApi.SendGroupMessage(context.FromGroup, "Done.");
                }
                else
                {
                    _mahuaApi.SendGroupMessage(context.FromGroup, "Permission Denied.");
                }
            }
            if (context.Message == "移除群")
            {
                if (_mahuaApi.GetGroupMemberInfo(context.FromGroup, context.FromQq).Authority ==
                    GroupMemberAuthority.Manager ||
                    _mahuaApi.GetGroupMemberInfo(context.FromGroup, context.FromQq).Authority ==
                    GroupMemberAuthority.Leader)
                {
                    Config.Instance.WFGroupList.Remove(context.FromGroup);
                    _mahuaApi.SendGroupMessage(context.FromGroup, "Done.");
                }
                else
                {
                    _mahuaApi.SendGroupMessage(context.FromGroup, "Permission Denied.");
                }
            }
        }
    }
}
