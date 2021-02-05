using System;
using System.Collections.Generic;
using System.Text;
using WFBot.Features.Utils;

namespace WFBot.Utils
{
    public struct GroupMessageSender
    {
        public GroupID GroupID;

        public GroupMessageSender(GroupID groupID)
        {
            this.GroupID = groupID;
        }

        public void SendMessage(string msg)
        {
            msg.SendToGroup(GroupID);
        }
    }
}
