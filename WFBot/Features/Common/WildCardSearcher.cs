using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GammaLibrary.Extensions;
using PininSharp;
using PininSharp.Searchers;
using WFBot.Features.Resource;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Features.Common
{
    public class WildCardSearcher
    {
        private WildcardAndSlang was => WFResources.WildcardAndSlang;
        private Sale[] sales => WFResources.WFBotTranslateData.Sale;
        private TreeSearcher<Sale> _searcher = new TreeSearcher<Sale>(SearcherLogic.Contain, PinIn.CreateDefault());
        public WildCardSearcher()
        {
            // 先记个笔记
            // 输入词 加后缀:一套 和 不做修改 两种输入搜索器
            // 如果结果只有一个, 则匹配成功
            // 如果结果多个, 则返回多个结果作为搜索建议
            UpdateSearcher();
            // 笔记就留在这里吧, 免得以后我看不懂了
        }
        public List<Sale> Search(string text)
        {
            return _searcher.Search(text.Format()).Distinct().ToList();
        }
        public void UpdateSearcher()
        {
            _searcher = new TreeSearcher<Sale>(SearcherLogic.Contain, PinIn.CreateDefault());
            foreach (var sale in sales)
            {
                var list = new List<string>();
                var original = sale.zh.Format();
                foreach (var slang in was.Slang)
                {
                    foreach (var value in slang.Value)
                    {
                        var formatted = original;
                        var formatted1 = formatted.Replace(slang.Key.Format(), value.Format());
                        formatted = formatted1;
                        for (int i = 0; i < was.CombinationSlang.Count + 1; i++)
                        {
                            foreach (var combinationslangs in was.CombinationSlang.DifferentCombinations(i))
                            {
                                foreach (var combinationslang in combinationslangs)
                                {
                                    var count = 0;
                                    var eva = new MatchEvaluator(match =>
                                    {
                                        count++;
                                        return count > combinationslang.Times ? match.Value : combinationslang.Pair.Value;
                                    });
                                    if (combinationslang.Condition.Case == Case.None || 
                                        (combinationslang.Condition.Case == Case.Include && combinationslang.Condition.Target.Any(t => formatted.Contains(t))) || 
                                        (combinationslang.Condition.Case == Case.Exclude && !combinationslang.Condition.Target.All(t => formatted.Contains(t))))
                                    {
                                        formatted = Regex.Replace(formatted, combinationslang.Pair.Key, eva,
                                            combinationslang.Reverse ? RegexOptions.RightToLeft : RegexOptions.None);
                                    }
                                }
                                foreach (var suffix in was.Suffixes)
                                {

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
                }

                list = list.Distinct().ToList();
                foreach (var item in list)
                {
                    _searcher.Put(item, sale);
                }
            }
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
