using Newbe.Mahua.MahuaEvents;
using System;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using Newbe.Mahua;
using WebSocket = WebSocketSharp.WebSocket;

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
            void Accept()
            {
                using (var robotSession = MahuaRobotManager.Instance.CreateSession())
                {
                    var api = robotSession.MahuaApi;
                    api.AcceptGroupJoiningInvitation(context.GroupJoiningRequestId, context.ToGroup, context.FromQq);
                }
            }

            void Reject()
            {
                using (var robotSession = MahuaRobotManager.Instance.CreateSession())
                {
                    var api = robotSession.MahuaApi;
                    api.RejectGroupJoiningInvitation(context.GroupJoiningRequestId, context.ToGroup, context.FromQq, "WFBot拒绝了你的申请.");
                }

            }
            if (HotUpdateInfo.PreviousVersion) return;

            if (Config.Instance.IsPublicBot)
            {
                WebSocket ws = null;
                ws = new WebSocket("ws://127.0.0.1:15790/CheckGroup", default(CancellationToken), 102392, null, null,
                    s =>
                    {
                        var result = Boolean.Parse(s.Data.ReadToEnd());
                        if (result)
                        {
                            Accept();
                        }
                        else
                        {
                            Reject();
                        }
                        ws.Dispose();
                        return Task.CompletedTask;
                    });
                ws.Connect().Wait();
                ws.Send(context.ToGroup).Wait();
            }


            Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(a => 
            {
                if (Config.Instance.AcceptInvitation)
                {
                    Accept();
                    if (!Config.Instance.WFGroupList.Contains(context.ToGroup))
                    {
                        Config.Instance.WFGroupList.Add(context.ToGroup);
                    }
                    Messenger.SendDebugInfo($"接受了来自{context.FromQq}邀请加入群{context.ToGroup}的邀请.");
                    Messenger.SendHelpdoc(context.ToGroup.ToGroupNumber());
                }
                else
                {
                    Reject();
                }
            });

        }
    }
}