using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using Newbe.Mahua;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TRKS.WF.QQBot
{
    public class WebSocketHandler
    {
        private WebSocketServer webserver = new WebSocketServer(IPAddress.Parse("127.0.0.1"), 15789);
        public WebSocketHandler()
        {
            Task.Factory.StartNew(() => {
                webserver.AddWebSocketService<GetGroup>("/GetGroup");
                webserver.AddWebSocketService<AddGroup>("/AddGroup");
                webserver.Start();
            }, TaskCreationOptions.LongRunning);
        }
    }

    public class GetGroup : WebSocketBehavior
    {
        protected override async Task OnMessage(MessageEventArgs e)
        {
            string result;
            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
            {

                var mahuaApi = robotSession.MahuaApi;
                var members = mahuaApi.GetGroupMemebersWithModel(e.Data.ReadToEnd()).Model.ToList();
                result = string.Join(" ", members.Select(g => g.Qq));
            }

            await Send(result);
        }
    }
    public class AddGroup : WebSocketBehavior
    {
        protected override async Task OnMessage(MessageEventArgs e)
        {
            var group = e.Data.ReadToEnd();
            if (!Config.Instance.WFGroupList.Contains(group))
            {
                Config.Instance.WFGroupList.Add(group);
                Config.Save();
                Messenger.SendDebugInfo($"{group}启用了通知功能.");
            }

        }
    }

}
