using System;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using WFBot.Features.Utils;
using WebSocket = WebSocketSharp.WebSocket;

namespace WFBot.MahuaEvents
{
    /// <summary>
    /// 入群邀请接收事件
    /// </summary>
    public class GroupJoiningInvitationReceivedMahuaEvent1
    {
        /*
        public void ProcessJoinGroupRequest()
        {
            void Accept()
            {
                
            }

            void Reject()
            {
               

            }
            //if (HotUpdateInfo.PreviousVersion) return;

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
                //ws.Send(context.ToGroup).Wait();
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
        */
    }
}