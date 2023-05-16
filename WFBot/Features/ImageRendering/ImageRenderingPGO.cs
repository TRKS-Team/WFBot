using System.Diagnostics;
using System.Xml.Linq;
using WFBot.Features.Commands;
using WFBot.Features.Common;
using WFBot.Features.Other;
using WFBot.Features.Resource;
using WFBot.Features.Timers;
using WFBot.Features.Timers.Base;
using WFBot.Features.Utils;
using WFBot.Utils;
using WFBot.WebUI;

namespace WFBot.Features.ImageRendering
{
    public class PGOCache
    {
        public static Dictionary<string, WMInfo> WmInfos = new();
        public static Dictionary<string, List<RivenAuction>> Rivens = new();
    }

    
    public class ImageRenderingPGO
    {
        static HttpClient hc = new HttpClient(new RetryHandler(new HttpClientHandler()));
        static string cacheDir = "WFCaches/ImagesCache";

        static object fileAccessLock = new();
        public static byte[] WMInfo(string code)
        {
            var path = Path.Combine(cacheDir, $"wminfo_{code}.png");
            if (File.Exists(path))
            {
                lock (fileAccessLock)
                {
                    return File.ReadAllBytes(path);
                }
            }

            return null;
        }

        public static byte[] Riven(string urlName)
        {
            var path = Path.Combine(cacheDir, $"riven_{urlName}.png");
            if (File.Exists(path))
            {
                lock (fileAccessLock)
                {
                    return File.ReadAllBytes(path);
                }
            }

            return null;
        }
        public static void Init()
        {
            if (Directory.Exists(cacheDir))
            {
                Directory.Delete(cacheDir, true);
            }
            Directory.CreateDirectory(cacheDir);
            /*Task.Run(async () =>
            {
                while (true)
                {
                    var start = DateTime.Now;
                    await Parallel.ForEachAsync(PGOData.WMInfo, new ParallelOptions(){MaxDegreeOfParallelism = 3}, async (s, c) =>
                    {
                        try
                        {
                            var info = await WMSearcher.GetWMInfo(s);

                            WMSearcher.OrderWMInfo(info, false);
                            WFResources.WFTranslator.TranslateWMOrder(info, s);
                            var image = await ImageRenderHelper.WMInfo(info, false, false);
                            var path = Path.Combine(cacheDir, $"wminfo_{s}.png");
                            lock (fileAccessLock)
                            {
                                PGOCache.WmInfos[s] = info;
                                File.WriteAllBytes(path, image);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"PGO 轮转出错: {s} {e.Message} {e}");
                        }
                    });
                    Console.WriteLine($"WMInfo PGO 完成一次轮转");
                    if (DateTime.Now - start < TimeSpan.FromMinutes(2))
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1));
                    }
                }
            });*/
            /*Task.Run(async () =>
            {
                var start = DateTime.Now;
                while (true)
                {
                    await Parallel.ForEachAsync(PGOData.Rivens, new ParallelOptions() {MaxDegreeOfParallelism = 3},
                        async (s, c) =>
                        {
                            try
                            {
                                var weapon = WFResources.Weaponinfos.FirstOrDefault(r => r.urlname == s);
                                if (weapon != null)
                                {
                                    var auctions = await CommandsHandler.GetRivenAuctions(weapon.urlname);
                                    var rivens = auctions.Take(Config.Instance.WFASearchCount).ToList();
                                    PGOCache.Rivens[s] = rivens;
                                    var image = ImageRenderHelper.RivenAuction(rivens, weapon);
                                    var path = Path.Combine(cacheDir, $"riven_{s}.png");
                                    lock (fileAccessLock)
                                    {
                                        File.WriteAllBytes(path, image);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"PGO 轮转出错: {s} {e.Message} {e}");
                            }
                        });
                    Console.WriteLine($"Riven PGO 完成一次轮转");
                    if (DateTime.Now - start < TimeSpan.FromMinutes(6))
                    {
                        await Task.Delay(TimeSpan.FromMinutes(5));
                    }
                }
            });*/
        }

        private static WFChineseAPI api => WFResources.WFChineseApi;
        static SemaphoreSlim tickSemaphore = new SemaphoreSlim(1);
        static string _invPath = Path.Combine(cacheDir, "inv.png");
        static string _cyclesPath = Path.Combine(cacheDir, "cycles.png");
        static string _f1Path = Path.Combine(cacheDir, "fissures.png");
        static string _f2Path = Path.Combine(cacheDir, "fissuresStorm.png");
        static string _f3Path = Path.Combine(cacheDir, "fissuresHard.png");
        static string _traderPath = Path.Combine(cacheDir, "trader.png");
        static string _sortiePath = Path.Combine(cacheDir, "sortie.png");

        public static byte[] Invasion()
        {
            lock (fileAccessLock)
                return File.Exists(_invPath) ? File.ReadAllBytes(_invPath) : null;
        }
        public static byte[] Cycles()
        {
            lock (fileAccessLock)
                return File.Exists(_cyclesPath) ? File.ReadAllBytes(_cyclesPath) : null;
        }
        public static byte[] Fissures()
        {
            lock (fileAccessLock)
                return File.Exists(_f1Path) ? File.ReadAllBytes(_f1Path) : null;
        }
        public static byte[] FissuresStorm()
        {
            lock (fileAccessLock)
                return File.Exists(_f2Path) ? File.ReadAllBytes(_f2Path) : null;
        }
        public static byte[] FissuresHard()
        {
            lock (fileAccessLock)
                return File.Exists(_f3Path) ? File.ReadAllBytes(_f3Path) : null;
        }
        public static byte[] Trader()
        {
            lock (fileAccessLock)
                return File.Exists(_traderPath) ? File.ReadAllBytes(_traderPath) : null;
        }
        public static byte[] Sortie()
        {
            lock (fileAccessLock)
                return File.Exists(_sortiePath) ? File.ReadAllBytes(_sortiePath) : null;
        }
        [CalledByTimer(typeof(ImageRenderingPGOTimer))]
        public static async void Tick()
        {
            if (!tickSemaphore.Wait(1000)) return;
            if (!Config.Instance.UseImagePGO) return;
            
            try
            {
                // AsyncContext.SetCommandIdentifier("入侵");
                // await WFBotCore.Instance.NotificationHandler.UpdateInvasionPool();
                // var inv = WFBotCore.Instance.NotificationHandler.InvasionPool;
                // var invPath = _invPath;
                // lock (fileAccessLock) File.WriteAllBytes(invPath, ImageRenderHelper.Invasion(inv.Where(i => !i.completed)));
                //
                // AsyncContext.SetCommandIdentifier("平原");
                // var cetuscycle = await api.GetCetusCycle();
                // var valliscycle = await api.GetVallisCycle();
                // var earthcycle = await api.GetEarthCycle();
                // var cambioncycle = await api.GetCambionCycle();
                // // 均衡时间差
                // cambioncycle.expiry += TimeSpan.FromSeconds(15);
                // valliscycle.expiry += TimeSpan.FromSeconds(15);
                // earthcycle.expiry += TimeSpan.FromSeconds(15);
                // cambioncycle.expiry += TimeSpan.FromSeconds(15);
                // var cyclesPath = _cyclesPath;
                // lock (fileAccessLock) File.WriteAllBytes(cyclesPath, ImageRenderHelper.Cycles(cetuscycle, valliscycle, earthcycle, cambioncycle));
                //
                // AsyncContext.SetCommandIdentifier("裂隙");
                // var fs = await api.GetFissures();
                // var fissures = fs.Where(fissure => fissure.active && !fissure.isStorm && !fissure.isHard).ToList();
                // var f1 = ImageRenderHelper.Fissures(fissures, 0);
                // var f1Path = _f1Path;
                // AsyncContext.SetCommandIdentifier("虚空风暴");
                // var fissuresStorm = fs.Where(fissure => fissure.active && fissure.isStorm).ToList();
                // var f2 = ImageRenderHelper.Fissures(fissuresStorm, 0);
                // var f2Path = _f2Path;
                // AsyncContext.SetCommandIdentifier("钢铁裂缝");
                // var fissuresHard = fs.Where(fissure => fissure.active && fissure.isHard).ToList();
                // var f3 = ImageRenderHelper.Fissures(fissuresHard, 0);
                // var f3Path = _f3Path;
                // lock (fileAccessLock) File.WriteAllBytes(f1Path, f1);
                // lock (fileAccessLock) File.WriteAllBytes(f2Path, f2);
                // lock (fileAccessLock) File.WriteAllBytes(f3Path, f3);
                //
                // AsyncContext.SetCommandIdentifier("虚空商人");
                // var trader = WFFormatter.ToString(await api.GetVoidTrader());
                // var traderPath = _traderPath;
                // lock (fileAccessLock) File.WriteAllBytes(traderPath, ImageRenderHelper.SimpleImageRendering(trader));
                //
                // AsyncContext.SetCommandIdentifier("突击");
                // var sortie = WFFormatter.ToString(await api.GetSortie());
                // var sortiePath = _sortiePath;
                // lock (fileAccessLock) File.WriteAllBytes(sortiePath, ImageRenderHelper.SimpleImageRendering(sortie));
                // Console.WriteLine("普通PGO完成一次轮转");
            }
            catch (Exception e)
            {
                Trace.WriteLine("PGO Tick Error:");
                Trace.WriteLine(e);
            }

            tickSemaphore.Release();
        }
    }

    public class PGOData
    {

        /*  var dic = new Dictionary<string, int>();
            foreach (var line in File.ReadAllLines("C:\\temp\\1.txt"))
            {
                if (line.StartsWith("WM#"))
                {
                    var s = line.Substring(3);
                    var items = new List<Sale>();
                    if (WMSearcher.Search(s, ref items) && items.Any())
                    {
                        var i = items.First();
                        if (!dic.ContainsKey(i.code))
                        {
                            dic[i.code] = 0;
                        }
                        dic[i.code]++;
                    }
                }
            }
            
            dic.Where(x => x.Value > 4).OrderByDescending(x => x.Value).Take(60).Select(x => x.Key).Print();
         */
        public static string[] WMInfo = new[]
        {
            "revenant_prime_set",
            "tatsu_prime_set",
            "mesa_prime_set",
            "phantasma_prime_set",
            "khora_prime_set",
            "exodia_contagion",
            "wukong_prime_set",
            "glaive_prime_set",
            "volt_prime_set",
            "saryn_prime_set",
            "nekros_prime_set",
            "rhino_prime_set",
            "arcane_energize",
            "primed_chamber",
            "garuda_prime_set",
            "ash_prime_set",
            "archon_vitality",
            "rubico_prime_set",
            "octavia_prime_set",/*
            "rifle_riven_mod_(veiled)",
            "harrow_prime_set",
            "nikana_prime_set",
            "transient_fortitude",
            "primed_reach",
            "titania_prime_set",
            "equinox_prime_set",
            "shedu_set",
            "inaros_prime_set",
            "mirage_prime_set",
            "loki_prime_set",
            "valkyr_prime_set",
            "melee_riven_mod_(veiled)",
            "khora_prime_neuroptics_blueprint",
            "revenant_prime_chassis_blueprint",
            "molt_augmented",
            "dual_keres_prime_set",
            "arcane_grace",
            "vauban_prime_set",
            "kronen_prime_set",
            "hespar_blade",
            "primed_flow",
            "frost_prime_set",
            "revenant_prime_blueprint",
            "tekko_prime_set",
            "vectis_prime_set",
            "blaze",
            "primed_charged_shell",
            "voidrig_set",
            "ember_prime_set",
            "corrupt_charge",
            "arcane_guardian",
            "primed_point_blank",
            "tatsu_prime_handle",
            "nezha_prime_set",
            "aeolak_set",
            "primed_continuity",
            "nidus_prime_set",
            "limbo_prime_set",
            "primed_firestorm",
            "ivara_prime_set"*/
        };

        /*
         *  var dic = new Dictionary<string, int>();
            foreach (var line in File.ReadAllLines("C:\\temp\\1.txt"))
            {
                if (line.StartsWith("Riven#"))
                {
                    var s = line.Substring(6).Format();
                    WeaponInfo[] weaponInfos = WFResources.Weaponinfos;
                    var weapons = weaponInfos.Where(r => r.zhname.Format() == s).ToList();
                    if (weapons.Any())
                    {
                        if (!dic.ContainsKey(weapons.First().urlname))
                        {
                            dic[weapons.First().urlname] = 0;
                        }

                        dic[weapons.First().urlname]++;
                    }
                }
            }
            
            dic.Where(x => x.Value > 4).OrderByDescending(x => x.Value).Take(80)
                .Select(x => x.Key)
                .Print();
         */
        public static string[] Rivens = new[]
        {
            "felarx",
            "phantasma",
            "tatsu",
            "vectis",
            "tombfinger",
            "sporelacer",
            "vermisplicer",
            "tenet_envoy",
            "laetum",
            "rubico",
            "shedu",/*
            "zarr",
            "glaive",
            "praedos",
            "kronen",
            "zenith",
            "knell",
            "phenmor",
            "tonkor",
            "nikana",
            "ogris",
            "pennant",
            "dual_keres",
            "hek",
            "hespar",
            "bubonico",
            "rabvee",
            "arca_plasmor",
            "cerata",
            "nukor",
            "plague_kripath",
            "tekko",
            "stropha",
            "guandao",
            "tenet_livia",
            "redeemer",
            "slaytra",
            "cedo",
            "nataruk",
            "heat_dagger",
            "arca_titron",
            "aeolak",
            "gram",
            "kronsh",
            "venka",
            "kuva_bramma",
            "lacera",
            "amphis",
            "epitaph",
            "destreza",
            "ambassador",
            "dark_sword",
            "tigris",
            "aegrit",
            "kohm",
            "kuva_chakkhurr",
            "xoris",
            "quassus",
            "akarius",
            "tenora",
            "pathocyst"*/
        };
    }
}
