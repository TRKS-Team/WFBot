using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFBot;
using WFBot.Connector;

namespace TestConnector
{
    public class Connector : WFBotConnectorBase
    {
        public override void Init()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var line = Console.ReadLine();
                    WFBotCore.Instance.OnMessage("fork", "fork", line);
                }
            });
        }

        public override void SendGroupMessage(string id, string message)
        {
            Console.WriteLine($"ID {id} message {message}");
        }

        public override void SendPrivateMessage(string id, string message)
        {
            Console.WriteLine($"ID {id} message {message}");
        }
    }
}
