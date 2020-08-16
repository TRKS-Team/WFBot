using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFBot;
using WFBot.Connector;
using WFBot.Features.Utils;

namespace TestConnector
{
    public class Connector : WFBotConnectorBase
    {
        public override void Init()
        {
        }

        public override void SendGroupMessage(GroupID groupID, string message)
        {
            Console.WriteLine($"ID {groupID} message {message}");
        }

        public override void SendPrivateMessage(UserID userID, string message)
        {
            Console.WriteLine($"ID {userID} message {message}");
        }

        public override void OnCommandLineInput(string content)
        {
            ReportGroupMessage("fork", "fork", content);
        }
    }
}
