using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFBot.Features.Utils;

namespace WFBot.Connector
{
    public abstract class WFBotConnectorBase
    {
        public abstract void Init();
        public abstract void SendGroupMessage(GroupID id, string message);
        public abstract void SendPrivateMessage(UserID id, string message);
        public virtual void OnCommandLineInput(string content) { }

        /*protected void ReportGroupMessage(GroupID groupID, UserID userID, string message)
        {
            WFBotCore.Instance.OnGroupMessage(groupID, userID, message);
        }
        protected void ReportFriendMessage(UserID userID, string message)
        {
            WFBotCore.Instance.OnFriendMessage(userID, message);
        }*/
    }
}
