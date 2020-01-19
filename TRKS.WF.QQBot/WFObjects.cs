using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;

namespace TRKS.WF.QQBot
{
    /*
        public class WikiQuery
        {
            public bool batchcomplete { get; set; }
            public Query query { get; set; }
        }

        public class Query
        {
            public Page[] pages { get; set; }
        }

        public class Page
        {
            public int pageid { get; set; }
            public int ns { get; set; }
            public string title { get; set; }
            public Revision[] revisions { get; set; }
        }

        public class Revision
        {
            public string contentformat { get; set; }
            public string contentmodel { get; set; }
            private string _content;

            public string content
            {
                get => _content;
                set
                {
                    _content = value;
                    wiki = value.JsonDeserialize<Wiki>();
                }
            }

            [JsonIgnore]
            public Wiki wiki
            {
                get;
                set;
                // ↑超牛逼的写法 
            }
        }

        public class Wiki
        {
            public Dictionary<string, string> Text { get; set; }
            public Dictionary<string, string> Category { get; set; }
        }*/


    /*    public class Kuva
        {
            public DateTime start { get; set; }
            public DateTime end { get; set; }
            public string missiontype { get; set; }
            public string solnode { get; set; }
            public Solnodedata solnodedata { get; set; }
            public DateTime realtime { get; set; }
        }

        public class Solnodedata
        {
            public string name { get; set; }
            public string tile { get; set; }
            public string planet { get; set; }
            public string enemy { get; set; }
            public string type { get; set; }
            public string node_type { get; set; }
            public bool archwing { get; set; }
            public bool sharkwing { get; set; }
        }*/




    public class RivenData
    {
        public string itemType { get; set; }
        public string compatibility { get; set; }
        public bool rerolled { get; set; }
        public float avg { get; set; }
        public float stddev { get; set; }
        public int min { get; set; }
        public int max { get; set; }
        public int pop { get; set; }
        public float median { get; set; }
    }

    public class Outpost
    {
        public OMission mission { get; set; }
        public DateTime activation { get; set; }
        public DateTime expiry { get; set; }
        public bool active { get; set; }
        public string id { get; set; }
    }

    public class OMission
    {
        public string node { get; set; }
        public string faction { get; set; }
        public string type { get; set; }
    }

    public class Kuva
    {
        public DateTime activation { get; set; }
        public DateTime expiry { get; set; }
        public string solnode { get; set; }
        public string node { get; set; }
        public string name { get; set; }
        public string tile { get; set; }
        public string planet { get; set; }
        public string enemy { get; set; }
        public string type { get; set; }
        public string node_type { get; set; }
        public bool archwing { get; set; }
        public bool sharkwing { get; set; }
    }

    public class Arbitration
    {
        public DateTime activation { get; set; }
        public DateTime expiry { get; set; }
        public string solnode { get; set; }
        public string node { get; set; }
        public string name { get; set; }
        public string tile { get; set; }
        public string planet { get; set; }
        public string enemy { get; set; }
        public string type { get; set; }
        public string node_type { get; set; }
        public bool archwing { get; set; }
        public bool sharkwing { get; set; }
    }


    public class EarthCycle
    {
        public string id { get; set; }
        public DateTime expiry { get; set; }
        public DateTime activation { get; set; }
        public bool isDay { get; set; }
        public string state { get; set; }
        public string timeLeft { get; set; }
    }

    public class Wiki
    {
        public Error error { get; set; }
        public bool batchcomplete { get; set; }
        public Continue _continue { get; set; }
        public Query query { get; set; }
    }


    public class Error
    {
        public string code { get; set; }
        public string info { get; set; }
        public string docref { get; set; }
    }

    public class Continue
    {
        public int sroffset { get; set; }
        public string _continue { get; set; }
    }

    public class Query
    {
        public Searchinfo searchinfo { get; set; }
        public Search[] search { get; set; }
    }

    public class Searchinfo
    {
        public int totalhits { get; set; }
    }

    public class Search
    {
        public int ns { get; set; }
        public string title { get; set; }
        public int pageid { get; set; }
        public int size { get; set; }
        public int wordcount { get; set; }
        public string snippet { get; set; }
        public DateTime timestamp { get; set; }
    }


    public class WFNightWave
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public string startString { get; set; }
        public DateTime expiry { get; set; }
        public bool active { get; set; }
        public int season { get; set; }
        public string tag { get; set; }
        public int phase { get; set; }
        public Params _params { get; set; }
        public object[] possibleChallenges { get; set; }
        public Activechallenge[] activeChallenges { get; set; }
        public string[] rewardTypes { get; set; }
    }

    public class Params
    {
        public int wgsc { get; set; }
        public float wsr { get; set; }
    }

    public class Activechallenge
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public string startString { get; set; }
        public DateTime expiry { get; set; }
        public bool active { get; set; }
        public bool isDaily { get; set; }
        public bool isElite { get; set; }
        public string desc { get; set; }
        public string title { get; set; }
        public int reputation { get; set; }
    }

    public class NightWave
    {
        public int id { get; set; }
        public string zh { get; set; }
        public string en { get; set; }
    }

    public class PersistentEnemie
    {
        public string id { get; set; }
        public string agentType { get; set; }
        public string locationTag { get; set; }
        public int rank { get; set; }
        public float healthPercent { get; set; }
        public int fleeDamage { get; set; }
        public int region { get; set; }
        public DateTime lastDiscoveredTime { get; set; }
        public string lastDiscoveredAt { get; set; }
        public bool isDiscovered { get; set; }
        public bool isUsingTicketing { get; set; }
        public string pid { get; set; }
    }

    public class WMInfoEx
    {
        public OrderEx[] orders { get; set; }
        public Info info { get; set; }
    }

    public class Info
    {
        public int ducats { get; set; }
        public int tradingTax { get; set; }
        public int advicePrice { get; set; }
        [JsonIgnore]
        public string enName { get; set; }
        [JsonIgnore]
        public string zhName { get; set; }
    }

    public class OrderEx
    {
        public string userName { get; set; }
        public int platinum { get; set; }
        public int quantity { get; set; }
        public string status { get; set; }
        public string itemName { get; set; }
        public int mod_Level { get; set; }
        public string order_Type { get; set; }
    }

    public class Event
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public string startString { get; set; }
        public DateTime expiry { get; set; }
        public bool active { get; set; }
        public int maximumScore { get; set; }
        public string faction { get; set; }
        public string description { get; set; }
        public string node { get; set; }
        public object[] concurrentNodes { get; set; }
        public string victimNode { get; set; }
        public string scoreLocTag { get; set; }
        public EventReward[] rewards { get; set; }
        public bool expired { get; set; }
        public string health { get; set; }
        public string asString { get; set; }
    }

    public class EventReward
    {
        public string[] items { get; set; }
        public object[] countedItems { get; set; }
        public int credits { get; set; }
        public string asString { get; set; }
        public string itemString { get; set; }
        public string thumbnail { get; set; }
        public int color { get; set; }
    }

    public class RivenInfo
    {
        public string item_Name { get; set; }
        public string item_Cate { get; set; }
        public string item_Class { get; set; }
        public string item_Property { get; set; }
        public int item_Price { get; set; }
        public string user_Name { get; set; }
        public string item_Id { get; set; }
        public int user_Status { get; set; }
        public int item_Status { get; set; }
        public int item_Level { get; set; }
        public int isSell { get; set; }
        public int item_ResetNum { get; set; }
        public int last_In { get; set; }
        public int lockingNum { get; set; }
        public string lockingUser { get; set; }
        public int business_Process { get; set; }
        public int customer_Process { get; set; }
        public string success_UserId { get; set; }
        public int item_Dan { get; set; }
        public string item_Polarity { get; set; }
        public object item_Platform { get; set; }
        public int isVeiled { get; set; }
        public int user_Level { get; set; }
        public int last_Update { get; set; }
        public object user_Mail { get; set; }
    }


    public class AccessToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
    }

    public class VallisCycle
    {
        public string id { get; set; }
        public DateTime expiry { get; set; }
        public bool isWarm { get; set; }
        public string timeLeft { get; set; }
        public string shortString { get; set; }
    }

    public class Fissure
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public string startString { get; set; }
        public DateTime expiry { get; set; }
        public bool active { get; set; }
        public string node { get; set; }
        public string missionType { get; set; }
        public string enemy { get; set; }
        public string tier { get; set; }
        public int tierNum { get; set; }
        public bool expired { get; set; }
        public string eta { get; set; }
    }


    public class SyndicateMission
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public string startString { get; set; }
        public DateTime expiry { get; set; }
        public bool active { get; set; }
        public string syndicate { get; set; }
        public string[] nodes { get; set; }
        public Job[] jobs { get; set; }
        public string eta { get; set; }
    }

    public class Job
    {
        public string id { get; set; }
        public string[] rewardPool { get; set; }
        public string type { get; set; }
        public int[] enemyLevels { get; set; }
        public int[] standingStages { get; set; }
    }

    public class WMInfo
    {
        public Payload payload { get; set; }
        public Include include { get; set; }
    }

    public class Payload
    {
        public Order[] orders { get; set; }
    }

    public class Order
    {
        public bool visible { get; set; }
        public int quantity { get; set; }
        public User user { get; set; }
        public double platinum { get; set; }
        public DateTime creation_date { get; set; }
        public string region { get; set; }
        public DateTime last_update { get; set; }
        public string order_type { get; set; }
        public string platform { get; set; }
        public string id { get; set; }
    }

    public class User
    {
        public string ingame_name { get; set; }
        public DateTime? last_seen { get; set; }
        public float reputation_bonus { get; set; }
        public float reputation { get; set; }
        public string region { get; set; }
        public string status { get; set; }
        public string id { get; set; }
        public string avatar { get; set; }
    }

    public class Include
    {
        public Item item { get; set; }
    }

    public class Item
    {
        public string id { get; set; }
        public Items_In_Set[] items_in_set { get; set; }
    }

    public class Items_In_Set
    {
        public int ducats { get; set; }
        public string id { get; set; }
        public string sub_icon { get; set; }
        public string icon_format { get; set; }
        public string[] tags { get; set; }
        public Zh zh { get; set; }
        public string thumb { get; set; }
        public En en { get; set; }
        public Ko ko { get; set; }
        public Fr fr { get; set; }
        public int trading_tax { get; set; }
        public bool set_root { get; set; }
        public De de { get; set; }
        public Sv sv { get; set; }
        public string icon { get; set; }
        public Ru ru { get; set; }
        public string url_name { get; set; }
    }

    public class Zh
    {
        public string description { get; set; }
        public Drop[] drop { get; set; }
        public string item_name { get; set; }
        public string codex { get; set; }
        public string wiki_link { get; set; }
    }

    public class Drop
    {
        public object link { get; set; }
        public string name { get; set; }
    }

    public class En
    {
        public string description { get; set; }
        public Drop1[] drop { get; set; }
        public string item_name { get; set; }
        public string codex { get; set; }
        public string wiki_link { get; set; }
    }

    public class Drop1
    {
        public object link { get; set; }
        public string name { get; set; }
    }

    public class Ko
    {
        public string description { get; set; }
        public Drop2[] drop { get; set; }
        public string item_name { get; set; }
        public string codex { get; set; }
        public string wiki_link { get; set; }
    }

    public class Drop2
    {
        public object link { get; set; }
        public string name { get; set; }
    }

    public class Fr
    {
        public string description { get; set; }
        public Drop3[] drop { get; set; }
        public string item_name { get; set; }
        public string codex { get; set; }
        public string wiki_link { get; set; }
    }

    public class Drop3
    {
        public object link { get; set; }
        public string name { get; set; }
    }

    public class De
    {
        public string description { get; set; }
        public Drop4[] drop { get; set; }
        public string item_name { get; set; }
        public string codex { get; set; }
        public string wiki_link { get; set; }
    }

    public class Drop4
    {
        public object link { get; set; }
        public string name { get; set; }
    }

    public class Sv
    {
        public string description { get; set; }
        public Drop5[] drop { get; set; }
        public string item_name { get; set; }
        public string codex { get; set; }
        public string wiki_link { get; set; }
    }

    public class Drop5
    {
        public object link { get; set; }
        public string name { get; set; }
    }

    public class Ru
    {
        public string description { get; set; }
        public Drop6[] drop { get; set; }
        public string item_name { get; set; }
        public string wiki_link { get; set; }
    }

    public class Drop6
    {
        public object link { get; set; }
        public string name { get; set; }
    }



    public class VoidTrader
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public DateTime expiry { get; set; }
        public string character { get; set; }
        public string location { get; set; }
        public Inventory[] inventory { get; set; }
        public string psId { get; set; }
        public bool active { get; set; }
        public string startString { get; set; }
        public string endString { get; set; }
    }

    public class Inventory
    {
        public string item { get; set; }
        public int ducats { get; set; }
        public int credits { get; set; }
    }


    public class WFApi
    {
        public Dict[] Dict { get; set; }
        public Sale[] Sale { get; set; }
        public Alert[] Alert { get; set; }
        public Invasion[] Invasion { get; set; }
        public Riven[] Riven { get; set; }
        public Relic[] Relic { get; set; }
        public Modifier[] Modifier { get; set; }
        public NightWave[] NightWave { get; set; }
    }

    public class Modifier
    {
        public int id { get; set; }
        public string zh { get; set; }
        public string en { get; set; }
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


    public class Relic
    {
        public int Id { get; set; }
        public string Tier { get; set; }
        public string RelicName { get; set; }
        public string Rewards { get; set; }
        public string Name { get; set; }
    }

    public class Sortie
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public DateTime expiry { get; set; }
        public string rewardPool { get; set; }
        public Variant[] variants { get; set; }
        public string boss { get; set; }
        public string faction { get; set; }
        public bool expired { get; set; }
        public string eta { get; set; }
    }

    public class Variant
    {
        public string boss { get; set; }
        public string planet { get; set; }
        public string missionType { get; set; }
        public string modifier { get; set; }
        public string modifierDescription { get; set; }
        public string node { get; set; }
    }
    public class CetusCycle
    {
        public string ID { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsDay { get; set; }
        public string TimeLeft { get; set; }
        public bool IsCetus { get; set; }
        public string ShortString { get; set; }
    }



    /*   public class WFInvasions
       {
           public WFInvasion[] Property1 { get; set; }
       }

       public class WFInvasion
       {
           public string id { get; set; }
           public DateTime activation { get; set; }
           public string startString { get; set; }
           public string node { get; set; }
           public string desc { get; set; }
           public Attackerreward attackerReward { get; set; }
           public string attackingFaction { get; set; }
           public Defenderreward defenderReward { get; set; }
           public string defendingFaction { get; set; }
           public bool vsInfestation { get; set; }
           public int count { get; set; }
           public int requiredRuns { get; set; }
           public float completion { get; set; }
           public bool completed { get; set; }
           public string eta { get; set; }
           public string[] rewardTypes { get; set; }
       }

       public class Attackerreward
       {
           public object[] items { get; set; }
           public Counteditem[] countedItems { get; set; }
           public int credits { get; set; }
           public string asString { get; set; }
           public string itemString { get; set; }
           public string thumbnail { get; set; }
           public int color { get; set; }
       }

       public class Counteditem
       {
           public int count { get; set; }
           public string type { get; set; }
       }

       public class Defenderreward
       {
           public object[] items { get; set; }
           public Counteditem1[] countedItems { get; set; }
           public int credits { get; set; }
           public string asString { get; set; }
           public string itemString { get; set; }
           public string thumbnail { get; set; }
           public int color { get; set; }
       }

       public class Counteditem1
       {
           public int count { get; set; }
           public string type { get; set; }
       }*/

    public class WFAlert
    {
        public string Id { get; set; }
        public DateTime Activation { get; set; }
        public DateTime Expiry { get; set; }
        public AMission Mission { get; set; }
        public bool Expired { get; set; }
        public string Eta { get; set; }
        public string[] RewardTypes { get; set; }
    }

    public class AMission
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


    public class WFInvasion
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public string startString { get; set; }
        public string node { get; set; }
        public string desc { get; set; }
        public RewardInfo attackerReward { get; set; }
        public string attackingFaction { get; set; }
        public RewardInfo defenderReward { get; set; }
        public string defendingFaction { get; set; }
        public bool vsInfestation { get; set; }
        public int count { get; set; }
        public int requiredRuns { get; set; }
        public float completion { get; set; }
        public bool completed { get; set; }
        public string eta { get; set; }
        public string[] rewardTypes { get; set; }
    }

    public class RewardInfo
    {
        public object[] items { get; set; }
        public Counteditem[] countedItems { get; set; }
        public int credits { get; set; }
        public string asString { get; set; }
        public string itemString { get; set; }
        public string thumbnail { get; set; }
        public int color { get; set; }
    }

    public class Counteditem
    {
        public int count { get; set; }
        public string type { get; set; }
    }

}
