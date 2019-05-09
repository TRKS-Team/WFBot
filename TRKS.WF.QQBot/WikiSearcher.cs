using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using Settings;

namespace TRKS.WF.QQBot
{
    public class WikiSearcher
    {
        private WFApi api => WFResource.WFApi;
        private WFTranslator translator => WFResource.WFTranslator;
        public String Search(String word)
        {
            var sb = new StringBuilder();
            if (word.IsNullOrEmpty())
            {
                sb.AppendLine("为指挥官献上wiki的链接: https://warframe.huijiwiki.com/wiki/");
            }           
            var searchword = word;
            if (!searchword.Contains("prime"))
            {
                searchword = word.Replace("p", "prime");
                if (!api.Dict.Select(d => d.Zh.Format()).Contains(searchword) || !api.Dict.Select(d => d.En.Format()).Contains(searchword))
                {
                    searchword = word;
                }
            }

            foreach (var dict in api.Dict.Where(dict => dict.Zh.Format() == searchword || dict.En.Format() == searchword))
            {
                sb.AppendLine($"为指挥找到[{searchword}]的wiki链接: https://warframe.huijiwiki.com/wiki/{dict.En.Replace(" ", "_")}");
                return sb.ToString().Trim();
            }
            sb.AppendLine($"很抱歉，未找到[{word}]的wiki页面所以，为指挥官献上wiki链接: https://warframe.huijiwiki.com/wiki/");
            var similarlist = translator.GetSimilarItem(word, api.Dict);
            if (similarlist.Any())
            {
                sb.AppendLine("请问这下面有没有你要找的呢?（可尝试复制下面的名称来进行搜索");
                foreach (var item in similarlist)
                {
                    sb.AppendLine($"    {item}");
                }
            }

            
            return sb.ToString().Trim();
        }
    }
}
