using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRKS.WF.QQBot
{
    public class WFApi
    {
        public Dict[] Dict { get; set; }
        public Sale[] Sale { get; set; }
        public Alert[] Alert { get; set; }
        public Invasion[] Invasion { get; set; }
        public Riven[] Riven { get; set; }
        public Statuscode[] StatusCode { get; set; }
        public Relic[] Relic { get; set; }
    }

    public class Dict
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Zh { get; set; }
        public string En { get; set; }
    }

    public class Sale
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Search { get; set; }
        public string Zh { get; set; }
        public string En { get; set; }
    }

    public class Alert
    {
        public int Id { get; set; }
        public string Zh { get; set; }
        public string En { get; set; }
    }

    public class Invasion
    {
        public int Id { get; set; }
        public string Zh { get; set; }
        public string En { get; set; }
    }

    public class Riven
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Level { get; set; }
        public float Ratio { get; set; }
    }

    public class Statuscode
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Zh { get; set; }
        public string En { get; set; }
    }

    public class Relic
    {
        public int Id { get; set; }
        public string Tier { get; set; }
        public string RelicName { get; set; }
        public string Rewards { get; set; }
        public string Name { get; set; }
    }
    
    /*
    public class WFAlerts // 某个好朋友让我改成大写，好习惯
    {
        public string Id { get; set; }
        public DateTime Activation { get; set; }
        public DateTime Expiry { get; set; }
        public Mission Mission { get; set; }
        public bool Expired { get; set; }
        public string Eta { get; set; }
        public string[] RewardTypes { get; set; }
        public DateTime GetRealTime()
        {
            return Expiry + TimeSpan.FromHours(8);
        }
    }

    public class Mission
    {
        public string Description { get; set; }
        public string Node { get; set; }
        public string Type { get; set; }
        public string Faction { get; set; }
        public Reward Reward { get; set; }
        public int MinEnemyLevel { get; set; }
        public int MaxEnemyLevel { get; set; }
        public bool Nightmare { get; set; }
        public bool ArchwingRequired { get; set; }
        public int MaxWaveNum { get; set; }
    }

    public class Reward
    {
        public string[] Items { get; set; }
        public CountedItem[] CountedItems { get; set; }
        public int Credits { get; set; }
        public string AsString { get; set; }
        public string ItemString { get; set; }
        public string Thumbnail { get; set; }
        public int Color { get; set; }
    }

    public class CountedItem
    {
        public int Count { get; set; }
        public string Type { get; set; }
    }
    */
}
