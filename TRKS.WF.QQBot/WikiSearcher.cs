using System.Linq;
using System.Text;
using GammaLibrary.Extensions;
using Settings;

namespace TRKS.WF.QQBot
{
    public class WikiSearcher
    {
        private static WFTranslator translator => WFResource.WFTranslator;
        private const string wikilink = "https://warframe.huijiwiki.com/wiki/";
        public string Search(string word)
        {
            if (word.IsNullOrEmpty())
            {
                return $"为指挥官献上wiki的链接: {wikilink}";
            }
            if (translator.ContainsWikiword(word.Format()))
            {
                return $"为指挥找到[{word}]的wiki链接: {wikilink + word}";
            }
            var sb = new StringBuilder();
            sb.AppendLine($"Wiki页面 {word} 不存在.");
            var similarlist = translator.GetSimilarItem(word.Format(), "wiki");
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
