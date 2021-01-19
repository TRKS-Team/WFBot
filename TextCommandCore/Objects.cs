using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextCommandCore
{
    public class TargetID
    {
        public string ID { get; }

        public TargetID(string id)
        {
            ID = id;
        }

        public static implicit operator TargetID(string id)
        {
            return new TargetID(id);
        }

        public static implicit operator string(TargetID id)
        {
            return id.ID;
        }
    }

    public class Message
    {
        public string Content { get; }

        public Message(string content)
        {
            Content = content;
        }

        public static implicit operator Message(string msg)
        {
            return new Message(msg);
        }

        public static implicit operator string(Message msg)
        {
            return msg.Content;
        }
    }
}
