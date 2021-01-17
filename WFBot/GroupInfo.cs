namespace WFBot.Events
{
    public class GroupInfo
    {
        public string Name { get;  }
        public string ID { get;  }

        public GroupInfo(string name, string id)
        {
            Name = name;
            ID = id;
        }
    }
}