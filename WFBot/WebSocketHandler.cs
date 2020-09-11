using System.Net;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using WebSocketSharp;
using WebSocketSharp.Server;
using WFBot.Features.Utils;

namespace WFBot
{
    public class WebSocketHandler
    {
        private WebSocketServer webserver = new WebSocketServer(IPAddress.Loopback, 15789);
        public WebSocketHandler()
        {
            Task.Factory.StartNew(() => {
                //webserver.AddWebSocketService<GetGroup>("/GetGroup");
                webserver.AddWebSocketService<AddGroup>("/AddGroup");
                webserver.Start();
            }, TaskCreationOptions.LongRunning);
        }
    }
    // tOdO 我觉得你得重写这一块
    // mAybE 以后可能用不到这一块
    /*
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
    */
    public class AddGroup : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            var group = e.Data;
            if (!Config.Instance.WFGroupList.Contains(group))
            {
                Config.Instance.WFGroupList.Add(group);
                Config.Save();
                Messenger.SendDebugInfo($"{group}启用了通知功能.");
            }

        }
    }

}
