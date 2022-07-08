using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using PininSharp;
using PininSharp.Searchers;
using SuffixTreeSharp;
using WFBot.Features.Resource;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Features.Common
{
    public class WildCardSearcherCache
    {
        public string CacheToken { get; set; } = "";
        public List<KeyValuePair<string, Sale>> SearchPairs { get; set; } = new List<KeyValuePair<string, Sale>>();
    }

    public class WildCardSearcher
    {
        private WildcardAndSlang was => WFResources.WildcardAndSlang;
        private Sale[] sales => WFResources.WFBotTranslateData.Sale;
        private TreeSearcher<Sale> _searcher = new TreeSearcher<Sale>(SearcherLogic.Contain, PinIn.CreateDefault());
        private GeneralizedSuffixTree _tree = new GeneralizedSuffixTree();
        private Dictionary<int, Sale> TreeSalesDic = new Dictionary<int, Sale>();


        public static async Task<WildCardSearcher> Create()
        {
            var obj = new WildCardSearcher();
            SpinWait.SpinUntil(() => WFResources.WildcardAndSlang != null && WFResources.WFBotTranslateData != null);
            // 先记个笔记
            // 输入词 加后缀:一套 和 不做修改 两种输入搜索器
            // 如果结果只有一个, 则匹配成功
            // 如果结果多个, 则返回多个结果作为搜索建议
            obj.UpdateSearcher();
            // 笔记就留在这里吧, 免得以后我看不懂了
            return obj;
        }

        public List<Sale> SuffixSearch(string text)
        {
            return _tree.Search(text.Format()).Select(t => TreeSalesDic[t]).Distinct().ToList();
        }
        public List<Sale> PininSearch(string text)
        {
            return _searcher.Search(text.Format()).Distinct().ToList();
        }
        public void UpdateSearcher()
        {
            var sw = new Stopwatch();
            Console.WriteLine("开始黑话辞典刷新");
            sw.Start();
            _searcher = new TreeSearcher<Sale>(SearcherLogic.Contain, PinIn.CreateDefault());
            _tree = new GeneralizedSuffixTree();
            
            var cacheToken = JsonSerializer.Serialize(sales, new JsonSerializerOptions() { WriteIndented = false }).SHA2().ToHexString() +
                             JsonSerializer.Serialize(was, new JsonSerializerOptions() { WriteIndented = false }).SHA2().ToHexString();

            var cachePath = "WFCaches/WildCardSearcherCache.json";
            WildCardSearcherCache cache;
            try
            {
                cache = File.Exists(cachePath) ? JsonSerializer.Deserialize<WildCardSearcherCache>(File.ReadAllText(cachePath)) : new WildCardSearcherCache();
            }
            catch (Exception e)
            {
                Trace.WriteLine($"读取黑话辞典缓存时遇到问题：{e}");
                cache = new WildCardSearcherCache();
            }

            if (cache.CacheToken == cacheToken)
            {
                foreach (var item in cache.SearchPairs)
                {
                    PutSearchers(item.Key, item.Value);
                }
                
                Trace.WriteLine($"黑话辞典穷举耗时（从缓存载入） '{sw.Elapsed.TotalSeconds:F3}s'");
                return;
            }

            void PutSearchers(string word, Sale item)
            {
                _searcher.Put(word, item);
                var index = _tree.HighestIndex + 1;
                _tree.Put(word, index);
                TreeSalesDic[index] = item;
            }
            cache.SearchPairs = new List<KeyValuePair<string, Sale>>();
            cache.CacheToken = cacheToken;

            var conbinationslangs = Enumerable.Range(0, was.CombinationSlang.Count)
                .Select(i => was.CombinationSlang.DifferentCombinations(i).Select(o => o.ToArray()).ToArray()).SelectMany(o => o).ToArray();

            var locker = new object();

            // 开始黑话辞典穷举优化...
            // 原耗时 14.4s
            // 去除多余内存分配及重复穷举运算
            //   => 4.4s
            // 更新至 .NET 6
            //   => 2.4s
            // 进一步移除重复运算
            //   => 2.0s
            // 更新为多线程
            //   => 0.9s
            // 切换为 Release 模式
            //   => 0.7s
            // 为了爱与和平 换回 .NET Core 3.1
            //   => 1.6s
            // 现在是5:47 AM 一位测试工程师安详地睡了过去
            // 不对啊我特喵的意识到 最好的优化应该是缓存
            // 写了个缓存
            //   => 0.1s
            // 提高计时器精度
            //   => 0.144s
            // 6:17 AM 晚安

            // 总是不相信 Parallel.Foreach 所以自己手写一个分区
            var parts = sales.ChunkBy(sales.Length / Environment.ProcessorCount);
            var tasks = new List<Task>(Environment.ProcessorCount);
            var copy = was.Slang.ToList();
            for (var index = 0; index < was.Slang.Count; index++)
            {
                was.Slang[index] = new KeyValuePair<string, List<string>>(was.Slang[index].Key.FormatFast(), was.Slang[index].Value.Select(v => v.FormatFast()).ToList());
            }

            foreach (var part in parts)
                tasks.Add(Task.Run(() =>
            {
                var count = new Container<int>(0);
                var cslanglast = new Container<SlangWithParams>(null);
                var eva = new MatchEvaluator(match =>
                {
                    count.Value++;
                    return count.Value > cslanglast.Value.Times ? match.Value : cslanglast.Value.Pair.Value;
                });

                var list = new HashSet<string>();
                foreach (var sale in part)
                {
                    var original = sale.zh.Format();

                    foreach (var slang in was.Slang)
                    {
                        foreach (var value in slang.Value)
                        {
                            var formatted = original;
                            var formatted1 = formatted.Replace(slang.Key, value);
                            formatted = formatted1;
                            foreach (var combinationslangs in conbinationslangs)
                            {
                                for (var i = 0; i < combinationslangs.Length; i++)
                                {
                                    var combinationslang = combinationslangs[i];
                                    cslanglast.Value = combinationslang;

                                    if (combinationslang.Condition.Case == Case.None ||
                                        (combinationslang.Condition.Case == Case.Include &&
                                         combinationslang.Condition.Target.Any(t => formatted.Contains(t))) ||
                                        (combinationslang.Condition.Case == Case.Exclude &&
                                         !combinationslang.Condition.Target.All(t => formatted.Contains(t))))
                                    {
                                        formatted = Regex.Replace(formatted, combinationslang.Pair.Key, eva,
                                            combinationslang.Reverse ? RegexOptions.RightToLeft : RegexOptions.None);
                                    }

                                    count.Value = 0;
                                }

                                for (var i = 0; i < was.Suffixes.Count; i++)
                                {
                                    var suffix = was.Suffixes[i];
                                    var result = formatted;
                                    formatted = formatted1; // 完成上次的一遍枚举后要归零
                                    if (!formatted.Contains(suffix))
                                    {
                                        result = formatted + suffix;
                                    }

                                    /*_searcher.Put(formatted, sale);*/
                                    list.Add(result);
                                }
                            }


                        }
                    }


                    foreach (var word in list)
                    {
                        lock (locker)
                        {
                            PutSearchers(word, sale);
                            cache.SearchPairs.Add(new KeyValuePair<string, Sale>(word, sale));
                        }
                    }
                    list.Clear();
                }
            }));

            Task.WaitAll(tasks.ToArray());
            was.Slang = copy;
            
            try
            {
                File.WriteAllText(cachePath, JsonSerializer.Serialize(cache, new JsonSerializerOptions { WriteIndented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
            }
            catch (Exception e)
            {
                Trace.WriteLine($"写入黑化辞典缓存失败：{e}");
            }
            sw.Stop();
            Trace.WriteLine($"黑话辞典穷举耗时 '{sw.Elapsed.TotalSeconds:F1}s'");
        }

    }
    public class WildcardAndSlang
    {
        public List<KeyValuePair<string, List<string>>> Slang = new List<KeyValuePair<string, List<string>>>();
        public List<SlangWithParams> CombinationSlang = new List<SlangWithParams>();
        public List<string> Suffixes = new List<string>();
    }
    public class SlangWithParams
    {
        public KeyValuePair<string, string> Pair = new KeyValuePair<string, string>();
        public bool Reverse = false;
        public int Times = Int32.MaxValue;
        public MatchCondition Condition = new MatchCondition();
    }

    public class MatchCondition
    {
        public List<string> Target;
        public Case Case = Case.None;
    }

    public enum Case
    {
        None,
        Include,
        Exclude
    }
}
