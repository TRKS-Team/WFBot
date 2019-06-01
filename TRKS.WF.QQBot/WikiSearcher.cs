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
        public string SendSearch(string word)
        {
            if (word.IsNullOrEmpty())
            {
                return $"为指挥官献上wiki的链接: {wikilink}";
            }

            var wiki = GetWiki(word);
            if (wiki.query.search.Select(s => s.title.Format()).Contains(word.Format()))
            {
                return $"为指挥官献上[{word}]的链接: {wikilink + translator.TranslateWikiSearchWord(word.Format())}";
            }
            var sb = new StringBuilder();
            sb.AppendLine($"Wiki页面 {word} 不存在.");
            var similarlist = wiki.query.search.Select(s => s.title).Take(3);
            if (similarlist.Any())
            {
                sb.AppendLine("请问这下面有没有你要找的呢?（可尝试复制下面的名称来进行搜索)");
                foreach (var item in similarlist)
                {
                    sb.AppendLine($"    {item}");
                }
            }

            return sb.ToString().Trim();
        }

        public Wiki GetWiki(string word)
        {
            return WebHelper.DownloadJson<Wiki>(
                $"https://warframe.huijiwiki.com/api.php?action=query&format=json&formatversion=2&list=search&srsearch={word}");
        }
    }
}
