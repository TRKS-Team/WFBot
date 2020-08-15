using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFBot.Connector
{
    public abstract class WFBotConnectorBase
    {
        public abstract void Init();
        public abstract void SendGroupMessage(string id, string message);
        public abstract void SendPrivateMessage(string id, string message);
        public virtual void OnCommandLineInput(string content) { }
    }
}
