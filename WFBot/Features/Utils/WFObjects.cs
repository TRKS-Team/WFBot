﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WFBot.Features.Resource;
using WFBot.Utils;

namespace WFBot.Features.Utils
{
    // 整个文件就是用来丢垃圾的
    public class WarframeUpdate
    {
        protected bool Equals(WarframeUpdate other)
        {
            return title == other.title && url == other.url;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WarframeUpdate)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(title, url);
        }

        public string title { get; set; }
        public string url { get; set; }
    }
    public class WeaponInfo
    {
        public string zhname { get; set; }
        public string enname { get; set; }
        public string urlname { get; set; }
    }
    public class WMAuction
    {
        public WMARiven[] ZHRivens => ZHRRivens.Value.Payload.Rivens;
        public WFResource<WMARivens> ZHRRivens;
        public WMARiven[] ENRivens => ENRRivens.Value.Payload.Rivens;
        public WFResource<WMARivens> ENRRivens;
        public WMAAttribute[] Attributes => RAttributes.Value.Payload.Attributes;
        public WFResource<WMAAttributes> RAttributes;
        /*public WMAItem[] Items => RItems.Value.Payload.Items;
        public WFResource<WMAItems> RItems;*/
    }
    public partial class WMAItems
    {
        protected bool Equals(WMAItems other)
        {
            return Equals(Payload, other.Payload);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WMAItems)obj);
        }

        public override int GetHashCode()
        {
            return (Payload != null ? Payload.GetHashCode() : 0);
        }

        [JsonProperty("payload")]
        public Payload3 Payload { get; set; }
    }

    public partial class Payload3
    {
        private WMAItem[] _items;

        [JsonProperty("items")]
        public WMAItem[] Items
        {
            get => _items;
            set
            {
                _items = value.OrderBy(i => i.Id).ToArray();
            }
        }
    }

    public partial class WMAItem
    {
        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("item_name")]
        public string ItemName { get; set; }

        [JsonProperty("url_name")]
        public string UrlName { get; set; }
    }
    public partial class WMARivens
    {
        protected bool Equals(WMARivens other)
        {
            return Equals(Payload, other.Payload);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WMARivens)obj);
        }

        public override int GetHashCode()
        {
            return (Payload != null ? Payload.GetHashCode() : 0);
        }

        [JsonProperty("payload", NullValueHandling = NullValueHandling.Ignore)]
        public Payload2 Payload { get; set; }
    }

    public partial class Payload2
    {
        private WMARiven[] _rivens;

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public WMARiven[] Rivens
        {
            get => _rivens;
            set
            {
                _rivens = value.OrderBy(r => r.Id).ToArray();
            }
        }
    }

    public partial class WMARiven
    {
        [JsonProperty("url_name", NullValueHandling = NullValueHandling.Ignore)]
        public string UrlName { get; set; }

        [JsonProperty("riven_type", NullValueHandling = NullValueHandling.Ignore)]
        public RivenType? RivenType { get; set; }

        [JsonProperty("item_name", NullValueHandling = NullValueHandling.Ignore)]
        public string ItemName { get; set; }

        [JsonProperty("group", NullValueHandling = NullValueHandling.Ignore)]
        public Group1? Group { get; set; }

        [JsonProperty("icon_format", NullValueHandling = NullValueHandling.Ignore)]
        public IconFormat? IconFormat { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("thumb", NullValueHandling = NullValueHandling.Ignore)]
        public string Thumb { get; set; }

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string Icon { get; set; }
    }

    public enum Group1 { Archgun, Kitgun, Melee, Primary, Secondary, Sentinel, Zaw };

    public enum IconFormat { Land, Port };

    public enum RivenType { Kitgun, Melee, Pistol, Rifle, Shotgun, Zaw };
    public partial class RivenAuctions
    {
        [JsonProperty("payload")]
        public Payload4 Payload { get; set; }
    }

    public partial class Payload4
    {
        [JsonProperty("auctions")]
        public List<RivenAuction> Auctions { get; set; }
    }

    public partial class RivenAuction
    {
        [JsonProperty("starting_price")]
        public long? StartingPrice { get; set; }

        [JsonProperty("buyout_price")]
        public long? BuyoutPrice { get; set; }

        [JsonProperty("minimal_reputation")]
        public long MinimalReputation { get; set; }

        [JsonProperty("private")]
        public bool Private { get; set; }

        [JsonProperty("visible")]
        public bool Visible { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("item")]
        public Item1 Item { get; set; }

        [JsonProperty("owner")]
        public Owner Owner { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("closed")]
        public bool Closed { get; set; }

        [JsonProperty("top_bid")]
        public long? TopBid { get; set; }

        [JsonProperty("winner")]
        public string Winner { get; set; }

        [JsonProperty("is_marked_for")]
        public object IsMarkedFor { get; set; }

        [JsonProperty("marked_operation_at")]
        public object MarkedOperationAt { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }

        [JsonProperty("updated")]
        public DateTimeOffset Updated { get; set; }

        [JsonProperty("note_raw")]
        public string NoteRaw { get; set; }

        [JsonProperty("is_direct_sell")]
        public bool IsDirectSell { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class Item1
    {
        [JsonProperty("mastery_level")]
        public long MasteryLevel { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("mod_rank")]
        public long ModRank { get; set; }

        [JsonProperty("weapon_url_name")]
        public string WeaponUrlName { get; set; }

        [JsonProperty("re_rolls")]
        public long ReRolls { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("attributes")]
        public List<AuctionAttribute> Attributes { get; set; }

        [JsonProperty("polarity")]
        public string Polarity { get; set; }
    }

    public partial class AuctionAttribute
    {
        [JsonProperty("positive")]
        public bool Positive { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("url_name")]
        public string UrlName { get; set; }
    }

    public partial class Owner
    {
        [JsonProperty("reputation")]
        public long Reputation { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("last_seen")]
        public DateTimeOffset LastSeen { get; set; }

        [JsonProperty("ingame_name")]
        public string IngameName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }
    }

    public partial class WMAAttributes
    {
        protected bool Equals(WMAAttributes other)
        {
            return Equals(Payload, other.Payload);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WMAAttributes)obj);
        }

        public override int GetHashCode()
        {
            return (Payload != null ? Payload.GetHashCode() : 0);
        }

        [JsonProperty("payload", NullValueHandling = NullValueHandling.Ignore)]
        public Payload1 Payload { get; set; }
    }

    public partial class Payload1
    {
        private WMAAttribute[] _attributes;

        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public WMAAttribute[] Attributes
        {
            get => _attributes;
            set
            {
                _attributes = value.OrderBy(a => a.Id).ToArray();
            }
        }
    }

    public partial class WMAAttribute
    {
        [JsonProperty("group", NullValueHandling = NullValueHandling.Ignore)]
        public Group? Group { get; set; }

        [JsonProperty("suffix")]
        public string Suffix { get; set; }

        [JsonProperty("negative_only", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NegativeOnly { get; set; }

        [JsonProperty("effect", NullValueHandling = NullValueHandling.Ignore)]
        public string Effect { get; set; }

        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("units")]
        public Units? Units { get; set; }

        [JsonProperty("url_name", NullValueHandling = NullValueHandling.Ignore)]
        public string UrlName { get; set; }

        [JsonProperty("exclusive_to")]
        public List<string> ExclusiveTo { get; set; }

        [JsonProperty("positive_is_negative", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PositiveIsNegative { get; set; }
    }

    public enum Group { Default, Melee, Top };

    public enum Units { Percent, Seconds };
    public class WFContentApi
    {
        public ExportRelicArcane[] ExportRelicArcanes => RExportRelicArcanes.Value;
        public WFResource<ExportRelicArcane[]> RExportRelicArcanes;
    }

    public partial class ExportRelicArcaneZh
    {
        public ExportRelicArcane[] ExportRelicArcane { get; set; }
    }

    public partial class ExportRelicArcane
    {
        public string Name { get; set; }
        public RelicReward[] RelicRewards { get; set; }
#if DEBUG
       public string UniqueName { get; set; }
       public bool CodexSecret { get; set; }
       public string Description { get; set; }
       public bool? ExcludeFromCodex { get; set; }
       public string Rarity { get; set; }
       public LevelStat[] LevelStats { get; set; }
#endif
    }

    public partial class LevelStat
    {
        public string[] Stats { get; set; }
    }

    public partial class RelicReward
    {
        public string RewardName { get; set; }
        public string Rarity { get; set; }
        public long Tier { get; set; }
        public long ItemCount { get; set; }
    }

    public class WFCD_All
    {
        public string name { get; set; }
        public List<Component> components { get; set; } = new List<Component>();
#if DEBUG // 很鸡巴弱智, 这下面一堆东西的都会null/混用类型, 如果遇到问题就关掉DEBUG模式
// 这个注释是我写的吗? 我又再次遇到这个问题了, 可是我得用DEBUG模式的功能 ah, 在未来换个数据源吧
// TODO 换数据源
// 笑死 CY还因为这个问题专门写了个Commit, 以为机器人开不起是这玩意造成的
        public string uniqueName { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string imageName { get; set; }
        public string category { get; set; }
        public bool tradable { get; set; }
        public bool excludeFromCodex { get; set; }
        public string rarity { get; set; }
        //public WFCD_Levelstat[] levelStats { get; set; }
        public bool showInInventory { get; set; }
        public int systemIndex { get; set; }
        public string systemName { get; set; }
        public int nodeType { get; set; }
        public int masteryReq { get; set; }
        public int missionIndex { get; set; }
        public int factionIndex { get; set; }
        public int minEnemyLevel { get; set; }
        public int maxEnemyLevel { get; set; }
        public string polarity { get; set; }
        public int baseDrain { get; set; }
        public int fusionLimit { get; set; }
        public string compatName { get; set; }
        public bool isAugment { get; set; }
        public string wikiaThumbnail { get; set; }
        public string wikiaUrl { get; set; }
        public bool transmutable { get; set; }
        public Patchlog[] patchlogs { get; set; }
        public WFCD_Drop[] drops { get; set; }
        public float[] damagePerShot { get; set; }
        public float totalDamage { get; set; }
        public float? criticalChance { get; set; }
        public float? criticalMultiplier { get; set; }
        public float procChance { get; set; }
        public float fireRate { get; set; }
        public string productCategory { get; set; }
        public int slot { get; set; }
        public float accuracy { get; set; }
        public float omegaAttenuation { get; set; }
        public string noise { get; set; }
        public string trigger { get; set; }
        public int magazineSize { get; set; }
        public float reloadTime { get; set; }
        public int multishot { get; set; }
        public int buildPrice { get; set; }
        public int buildTime { get; set; }
        public int skipBuildTimePrice { get; set; }
        public int buildQuantity { get; set; }
        public bool consumeOnBuild { get; set; }
        public string ammo { get; set; }
        public Areaattack areaAttack { get; set; }
        public object damage { get; set; }
        public Damagetypes damageTypes { get; set; }
        public object flight { get; set; }
        public object marketCost { get; set; }
        public string[] polarities { get; set; }
        public string projectile { get; set; }
        public string[] tags { get; set; }
        public int disposition { get; set; }
        public int blockingAngle { get; set; }
        public int comboDuration { get; set; }
        public float followThrough { get; set; }
        public float range { get; set; }
        public int slamAttack { get; set; }
        public int slamRadialDamage { get; set; }
        public int slamRadius { get; set; }
        public int slideAttack { get; set; }
        public int heavyAttackDamage { get; set; }
        public int heavySlamAttack { get; set; }
        public int heavySlamRadialDamage { get; set; }
        public int heavySlamRadius { get; set; }
        public float windUp { get; set; }
        public float channeling { get; set; }
        public string stancePolarity { get; set; }
        public int health { get; set; }
        public int shield { get; set; }
        public int armor { get; set; }
        public int stamina { get; set; }
        public int power { get; set; }
        public bool isUtility { get; set; }
        public int itemCount { get; set; }
        public string[] parents { get; set; }
        public string modSet { get; set; }
        public string rewardName { get; set; }
        public int tier { get; set; }
        public float probability { get; set; }
        public bool isExilus { get; set; }
        public Hexcolour[] hexColours { get; set; }
        public string releaseDate { get; set; }
        public string vaultDate { get; set; }
        public string estimatedVaultDate { get; set; }
        public bool vaulted { get; set; }
        public int fusionPoints { get; set; }
        public float sprintSpeed { get; set; }
        public Ability[] abilities { get; set; }
        public float chargeTime { get; set; }
        public Secondary secondary { get; set; }
        public Secondaryarea secondaryArea { get; set; }
        public float statusChance { get; set; }
        public Upgradeentry[] upgradeEntries { get; set; }
        public Availablechallenge[] availableChallenges { get; set; }
        public bool sentinel { get; set; }
        public string passiveDescription { get; set; }
        public string aura { get; set; }
        public bool conclave { get; set; }
        public int color { get; set; }
        //public string introduced { get; set; }
        public string sex { get; set; }
        public float sprint { get; set; }
        public string[] exalted { get; set; }
        public float primeOmegaAttenuation { get; set; }
        public int binCount { get; set; }
        public int binCapacity { get; set; }
        public double fillRate { get; set; }
        public int durability { get; set; }
        public int repairRate { get; set; }
        public int[] capacityMultiplier { get; set; }
        public object[] specialities { get; set; }
        public int primeSellingPrice { get; set; }
        public int maxLevelCap { get; set; }
        public float[] modSetValues { get; set; }
#endif
    }

    public class Areaattack
    {
        public string name { get; set; }
        public Falloff falloff { get; set; }
        public float slash { get; set; }
        public float puncture { get; set; }
        public int? crit_chance { get; set; }
        public int blast { get; set; }
        public string damage { get; set; }
        public float heat { get; set; }
        public float? crit_mult { get; set; }
        public float status_chance { get; set; }
        public int toxin { get; set; }
        public string shot_type { get; set; }
        public int? shot_speed { get; set; }
        public float impact { get; set; }
        public float speed { get; set; }
        public Pellet pellet { get; set; }
        public int gas { get; set; }
        public int electricity { get; set; }
        public int viral { get; set; }
        public int corrosive { get; set; }
        public int radiation { get; set; }
        public int cold { get; set; }
        public int magnetic { get; set; }
        public int charge_time { get; set; }
    }

    public class Falloff
    {
        public int start { get; set; }
        public float end { get; set; }
        public float reduction { get; set; }
    }

    public class Pellet
    {
        public string name { get; set; }
        public int count { get; set; }
    }

    public class Damagetypes
    {
        public float impact { get; set; }
        public float slash { get; set; }
        public float puncture { get; set; }
        public int toxin { get; set; }
        public int electricity { get; set; }
        public int blast { get; set; }
        public int radiation { get; set; }
        public int cold { get; set; }
        public int heat { get; set; }
        public int magnetic { get; set; }
        public int corrosive { get; set; }
        public int viral { get; set; }
    }

    public class Secondary
    {
        public string name { get; set; }
        public Pellet1 pellet { get; set; }
        public Falloff1 falloff { get; set; }
        public int blast { get; set; }
        public string damage { get; set; }
        public float speed { get; set; }
        public float? crit_chance { get; set; }
        public float? crit_mult { get; set; }
        public float status_chance { get; set; }
        public object shot_type { get; set; }
        // holy sh*t
        // 混用类型真 nt
        // 这里 shot_type 可以为 int 和 string
        public float impact { get; set; }
        public float slash { get; set; }
        public float puncture { get; set; }
        public int heat { get; set; }
        public int? shot_speed { get; set; }
        public int electricity { get; set; }
        public float charge_time { get; set; }
        public int radiation { get; set; }
        public int toxin { get; set; }
        public int cold { get; set; }
        public int corrosive { get; set; }
    }

    public class Pellet1
    {
        public int count { get; set; }
        public string name { get; set; }
    }

    public class Falloff1
    {
        public int start { get; set; }
        public float end { get; set; }
        public float reduction { get; set; }
    }

    public class Secondaryarea
    {
        public string name { get; set; }
        public float status_chance { get; set; }
        public Pellet2 pellet { get; set; }
        public int blast { get; set; }
        public string damage { get; set; }
        public int radius { get; set; }
        public int radiation { get; set; }
        public int toxin { get; set; }
        public int electricity { get; set; }
        public int impact { get; set; }
        public float slash { get; set; }
        public float puncture { get; set; }
        public int heat { get; set; }
        public int magnetic { get; set; }
        public int cold { get; set; }
        public int viral { get; set; }
        public int duration { get; set; }
        public float speed { get; set; }
    }

    public class Pellet2
    {
        public string name { get; set; }
        public int count { get; set; }
    }

    public class WFCD_Levelstat
    {
        public string[] stats { get; set; }
    }

    public class Patchlog
    {
        public string name { get; set; }
        public DateTime date { get; set; }
        public string url { get; set; }
        public string additions { get; set; }
        public string changes { get; set; }
        public string fixes { get; set; }
        public string imgUrl { get; set; }
    }

    public class WFCD_Drop
    {
        public string location { get; set; }
        public string type { get; set; }
        public float? chance { get; set; }
        public string rarity { get; set; }
    }

    public class Component
    {
        public string uniqueName { get; set; }
        public string name { get; set; }
#if DEBUG
        public string description { get; set; }
        public int itemCount { get; set; }
        public string imageName { get; set; }
        public bool tradable { get; set; }
        public WFCD_Drop1[] drops { get; set; }
        public bool excludeFromCodex { get; set; }
        public float[] damagePerShot { get; set; }
        public float totalDamage { get; set; }
        public float criticalChance { get; set; }
        public float criticalMultiplier { get; set; }
        public float procChance { get; set; }
        public float fireRate { get; set; }
        public int masteryReq { get; set; }
        public string productCategory { get; set; }
        public int slot { get; set; }
        public float accuracy { get; set; }
        public float omegaAttenuation { get; set; }
        public string noise { get; set; }
        public string trigger { get; set; }
        public int magazineSize { get; set; }
        public float reloadTime { get; set; }
        public int multishot { get; set; }
        public int ammo { get; set; }
        public string damage { get; set; }
        public Damagetypes1 damageTypes { get; set; }
        public int marketCost { get; set; }
        public string[] polarities { get; set; }
        public string projectile { get; set; }
        public string[] tags { get; set; }
        public string type { get; set; }
        public string wikiaThumbnail { get; set; }
        public string wikiaUrl { get; set; }
        public int disposition { get; set; }
        public int flight { get; set; }
        public int primeSellingPrice { get; set; }
        public int ducats { get; set; }
        public string releaseDate { get; set; }
        public string vaultDate { get; set; }
        public string estimatedVaultDate { get; set; }
        public int blockingAngle { get; set; }
        public int comboDuration { get; set; }
        public float followThrough { get; set; }
        public float range { get; set; }
        public int slamAttack { get; set; }
        public int slamRadialDamage { get; set; }
        public int slamRadius { get; set; }
        public int slideAttack { get; set; }
        public int heavyAttackDamage { get; set; }
        public int heavySlamAttack { get; set; }
        public int heavySlamRadialDamage { get; set; }
        public int heavySlamRadius { get; set; }
        public float windUp { get; set; }
        public float channeling { get; set; }
        public string stancePolarity { get; set; }
        public bool vaulted { get; set; }
        public float statusChance { get; set; }
#endif
    }

    public class Damagetypes1
    {
        public float impact { get; set; }
        public float slash { get; set; }
        public float puncture { get; set; }
        public int magnetic { get; set; }
        public int electricity { get; set; }
        public int heat { get; set; }
    }

    public class WFCD_Drop1
    {
        public string location { get; set; }
        public string type { get; set; }
        public float? chance { get; set; }
        public string rarity { get; set; }
    }

    public class Hexcolour
    {
        public string value { get; set; }
    }

    public class Ability
    {
        public string name { get; set; }
        public string description { get; set; }
    }

    public class Upgradeentry
    {
        public string tag { get; set; }
        public string prefixTag { get; set; }
        public string suffixTag { get; set; }
        public Upgradevalue[] upgradeValues { get; set; }
    }

    public class Upgradevalue
    {
        public float value { get; set; }
        public string locTag { get; set; }
        public bool reverseValueSymbol { get; set; }
    }

    public class Availablechallenge
    {
        public string fullName { get; set; }
        public string description { get; set; }
        public Complication[] complications { get; set; }
    }

    public class Complication
    {
        public string fullName { get; set; }
        public string description { get; set; }
        public string overrideTag { get; set; }
    }

    public class CambionCycle
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public DateTime expiry { get; set; }
        public string active { get; set; }
    }

    public class Lib
    {
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uniqueName { get; set; }
        public string thumb { get; set; }
    }


    public class AllRiven
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string thumb { get; set; }
        public string type { get; set; }
        public int rank { get; set; }
        public float modulus { get; set; }
    }


    public class Dict
    {
        public int id { get; set; }
        public string Zh { get; set; }
        public string En { get; set; }
    }

    public class Riven
    // of a thousand voices
    {
        public int id { get; set; }
        public string name { get; set; }
        [JsonIgnore]
        public string zhname { get; set; }
        public string thumb { get; set; }
        public string type { get; set; }
        public int rank { get; set; }
        public float modulus { get; set; }
    }

    public class Sale
    {
        public int id { get; set; }
        public string code { get; set; }
        public string main { get; set; }
        public string component { get; set; }
        public string zh { get; set; }
        public string en { get; set; }
        public string thumb { get; set; }

        protected bool Equals(Sale other)
        {
            return id == other.id && code == other.code && main == other.main && component == other.component && zh == other.zh && en == other.en && thumb == other.thumb;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Sale)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = id;
                hashCode = (hashCode * 397) ^ (code != null ? code.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (main != null ? main.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (component != null ? component.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (zh != null ? zh.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (en != null ? en.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (thumb != null ? thumb.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Sale left, Sale right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Sale left, Sale right)
        {
            return !Equals(left, right);
        }
    }

    public class Previous
    {
        public DateTime activation { get; set; }
        public DateTime expiry { get; set; }
    }

    public class SentientOutpost
    {
        public Mission mission { get; set; }
        public DateTime activation { get; set; }
        public DateTime expiry { get; set; }
        public bool active { get; set; }
        public string id { get; set; }
        public Previous previous { get; set; }
    }

    public class Mission
    {
        public string node { get; set; }
        public string faction { get; set; }
        public string type { get; set; }
    }

    public class SentientAnomaly
    {
        public string name { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public DateTime projection { get; set; }
    }
    public class RawSentientAnomaly
    {
        public string name { get; set; }
        public int start { get; set; }
        public int end { get; set; }
        public int projection { get; set; }
    }

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

    /*public class WMInfoEx
    {
        public OrderEx[] orders { get; set; }
        public Info info { get; set; }
        [JsonIgnore]
        public Sale sale { get; set; }
    }

    public class Info
    {
        public int ducats { get; set; }
        public int tradingTax { get; set; }
        public int advicePrice { get; set; }
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
    }*/
    public class Event
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public string startString { get; set; }
        public DateTime expiry { get; set; }
        public bool active { get; set; }
        public int? maximumScore { get; set; }
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
        public bool isStorm { get; set; }
        public bool isHard { get; set; }
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
        [JsonIgnore]
        public Sale sale { get; set; }
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

    public class WFBotApi
    {
        public Sale[] Sale => RSale.Value;
        public WFResource<Sale[]> RSale;
    }

    public class WFApi
    {
        public Dict[] Dict => RDict.Value;
        public WFResource<Dict[]> RDict;

        public Sale[] Sale => RSale.Value;
        public WFResource<Sale[]> RSale;

        public Invasion[] Invasion => RInvasion.Value;
        public WFResource<Invasion[]> RInvasion;

        public Riven[] Riven => RRiven.Value;
        public WFResource<Riven[]> RRiven;

        public NightWave[] NightWave => RNightWave.Value;
        public WFResource<NightWave[]> RNightWave;

        public AllRiven[] Allriven => RAllriven.Value;
        public WFResource<AllRiven[]> RAllriven;

        public Lib[] Lib => RLib.Value;
        public WFResource<Lib[]> RLib;

    }

    public class Modifier
    {
        public int id { get; set; }
        public string zh { get; set; }
        public string en { get; set; }
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


    public class ArchonHunt
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public string startString { get; set; }
        public DateTime expiry { get; set; }
        public bool active { get; set; }
        public string rewardPool { get; set; }
        public object[] variants { get; set; }
        public AHMission[] missions { get; set; }
        public string boss { get; set; }
        public string faction { get; set; }
        public bool expired { get; set; }
        public string eta { get; set; }
    }

    public class AHMission
    {
        public string node { get; set; }
        public string nodeKey { get; set; }
        public string type { get; set; }
        public string typeKey { get; set; }
        public bool nightmare { get; set; }
        public bool archwingRequired { get; set; }
        public bool isSharkwing { get; set; }
        public object[] advancedSpawners { get; set; }
        public object[] requiredItems { get; set; }
        public object[] levelAuras { get; set; }
    }

}
