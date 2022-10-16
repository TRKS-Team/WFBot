using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.CSharp;
using WFBot.Events;
using WFBot.Features.Other;
using WFBot.Features.Resource;
using WFBot.Utils;
using WFBot.Windows;

namespace WFBot.Features.Utils
{
    internal class StringInfo : IComparable<StringInfo>, IComparable
    {
        public string Name { get; set; }
        public int LevDistance { get; set; }

        public int CompareTo(StringInfo other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return LevDistance.CompareTo(other.LevDistance);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            return obj is StringInfo other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(StringInfo)}");
        }

        public static bool operator <(StringInfo left, StringInfo right)
        {
            return Comparer<StringInfo>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(StringInfo left, StringInfo right)
        {
            return Comparer<StringInfo>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(StringInfo left, StringInfo right)
        {
            return Comparer<StringInfo>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(StringInfo left, StringInfo right)
        {
            return Comparer<StringInfo>.Default.Compare(left, right) >= 0;
        }
    }

    public class WFChineseAPI
    {
        private WFTranslator translator => WFResources.WFTranslator;
        private WFCD_All[] all => WFResources.WFCDAll;
        private WFContentApi content => WFResources.WFContent;
        private WMAAttribute[] attributes => WFResources.WMAuction.Attributes;

        private static string platform => Config.Instance.Platform.GetSymbols().First();
        private static string WFstat => Config.Instance.UseWFBotProxy ? $"https://wfbot.cyan.cafe/api/WFBotProxy/{Config.Instance.WFBotProxyToken}*https://api.warframestat.us/{platform}" : $"https://api.warframestat.us/{platform}";

        public async Task<List<WFInvasion>> GetInvasions()
        {
            try
            {
                var invasions = await WebHelper.DownloadJsonAsync<List<WFInvasion>>(WFstat + "/invasions");
                foreach (var invasion in invasions)
                {
                    translator.TranslateInvasion(invasion);
                    invasion.activation = GetRealTime(invasion.activation);
                }

                return invasions;
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("入侵获取超时.");
            }

            return new List<WFInvasion>();
        }

        public async Task<List<WFAlert>> GetAlerts()
        {
            try
            {
                var alerts =
                    await WebHelper.DownloadJsonAsync<List<WFAlert>>(WFstat + "/alerts");
                foreach (var alert in alerts)
                {
                    translator.TranslateAlert(alert);
                    alert.Activation = GetRealTime(alert.Activation);
                    alert.Expiry = GetRealTime(alert.Expiry);
                }

                return alerts;
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("警报获取超时.");
            }
            catch (HttpRequestException)
            {
                // 啥也不做
            }
            catch (WebException)
            {
                // 啥也不做
            }
            catch (Exception e)
            {
                Messenger.SendDebugInfo($"警报获取报错: \r\n" +
                                        $"{e}");

            }
            return new List<WFAlert>();
        }
        public async Task<List<Kuva>> GetKuvaMissions()
        {
            var kuvas = await WebHelper.DownloadJsonAsync<List<Kuva>>(WFstat + "/kuva");
            translator.TranslateKuvaMission(kuvas);
            return kuvas;
        }

        public async Task<Arbitration> GetArbitrationMission()
        {
            var ar = await WebHelper.DownloadJsonAsync<Arbitration>(WFstat + "/arbitration");
            if (ar?.type == null) return null;
            
            translator.TranslateArbitrationMission(ar);
            return ar;
        }
        public async Task<WFNightWave> GetNightWave()
        {
            var wave = await WebHelper.DownloadJsonAsync<WFNightWave>(WFstat + "/nightwave");
            translator.TranslateNightWave(wave);
            return wave;
        }
        public async Task<CetusCycle> GetCetusCycle()
        {
            var cycle = await WebHelper.DownloadJsonAsync<CetusCycle>(WFstat + "/cetusCycle");
            cycle.Expiry = GetRealTime(cycle.Expiry);
            return cycle;
        }

        public async Task<VallisCycle> GetVallisCycle()
        {
            var cycle = await WebHelper.DownloadJsonAsync<VallisCycle>(WFstat + "/vallisCycle");
            cycle.expiry = GetRealTime(cycle.expiry);
            return cycle;
        }
        public async Task<EarthCycle> GetEarthCycle()
        {
            var cycle = await WebHelper.DownloadJsonAsync<EarthCycle>(WFstat + "/earthCycle");
            cycle.expiry = GetRealTime(cycle.expiry);
            return cycle;
        }

        public async Task<CambionCycle> GetCambionCycle()
        {
            var cycle = await WebHelper.DownloadJsonAsync<CambionCycle>(WFstat + "/cambionCycle");
            cycle.expiry = GetRealTime(cycle.expiry);
            return cycle;
        }


        public async Task<Sortie> GetSortie()
        {
            var sortie = await WebHelper.DownloadJsonAsync<Sortie>(WFstat + "/sortie");
            translator.TranslateSortie(sortie);
            return sortie;
        }

        public async Task<List<SyndicateMission>> GetSyndicateMissions()
        {
            var missions = await WebHelper.DownloadJsonAsync<List<SyndicateMission>>(WFstat + "/syndicateMissions");
            translator.TranslateSyndicateMission(missions);
            return missions;
        }
        public async Task<VoidTrader> GetVoidTrader()
        {
            var trader = await WebHelper.DownloadJsonAsync<VoidTrader>(WFstat + "/voidTrader");
            trader.activation = GetRealTime(trader.activation);
            trader.expiry = GetRealTime(trader.expiry);
            translator.TranslateVoidTrader(trader);
            return trader;
        }

        public async Task<List<Fissure>> GetFissures()
        {
            var fissures = await WebHelper.DownloadJsonAsync<List<Fissure>>(WFstat + "/fissures");
            translator.TranslateFissures(fissures);
            return fissures;
        }

        public async Task<List<Event>> GetEvents()
        {
            var events = await WebHelper.DownloadJsonAsync<List<Event>>(WFstat + "/events");
            translator.TranslateEvents(events);
            foreach (var @event in events)
            {
                @event.expiry = GetRealTime(@event.expiry);
            }

            return events;
        }

        public async Task<List<PersistentEnemie>> GetPersistentEnemies()
        {
            var enemies = await WebHelper.DownloadJsonAsync<List<PersistentEnemie>>(WFstat + "/persistentEnemies");
            translator.TranslatePersistentEnemies(enemies);
            return enemies;
        }

        public async Task<SentientOutpost> GetSentientOutpost()
        {
            var outpost = await WebHelper.DownloadJsonAsync<SentientOutpost>(WFstat + "/sentientOutposts");
            translator.TranslateSentientOutpost(outpost);
            return outpost;
        }

        public async Task<SentientAnomaly> GetSentientAnomaly()
        {
            var raw = await WebHelper.DownloadJsonAsync<RawSentientAnomaly>("https://semlar.com/anomaly.json");
            return translator.TranslateSentientAnomaly(raw);
        }

        public List<ExportRelicArcane> GetRelics(string word)
        {
            var items = content.ExportRelicArcanes.Where(ra =>
                ra.Name.Replace("遗物", "").Format().Contains(word.Format()));
            var result = new List<ExportRelicArcane>();
            foreach (var item in items)
            {
                if (result.All(r => r.Name != item.Name))
                {
                    result.Add(item);
                }
            }
            return result;

        }


        private static DateTime GetRealTime(DateTime time)
        {
            return time + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        }

    }


    /*
    public class ObjectType
    {
        public string Type;

        public ObjectType(string type)
        {
            Type = type;
        }
    }
    */
    public class TranslateResult
    {
        public string Result { get; set; }
        public bool IsTranslated { get; set; }
        public override string ToString()
        {
            return Result;
        }
    }
    public class Translator
    {
        private readonly Dictionary<string, string> dic;

        public Translator()
        {
            dic = new Dictionary<string, string>();
        }

        public Translator(Dictionary<string, string> dictionary)
        {
            dic = dictionary;
        }

        public TranslateResult Translate(string source)
        {
            if (source == null) return new TranslateResult {IsTranslated = false, Result = source};
            var translated = dic.ContainsKey(source);
            return new TranslateResult {IsTranslated = translated, Result = translated ? dic[source] : source};
        }

        public void AddEntry(string source, string target)
        {
            Debug.Assert(source != null);
            dic[source] = target;
        }

        public void Clear()
        {
            dic.Clear();
        }
    }
}
