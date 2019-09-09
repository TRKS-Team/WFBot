using System;
using System.Linq;
using System.Text;
using Settings;

namespace TRKS.WF.QQBot
{
    public class WikiSearcher
    {
        private const string wikilink = "https://warframe.huijiwiki.com/wiki/";


        public string SendSearch(string word)
        {

            if (word == "wiki")
            {
                return $"为指挥官献上wiki的链接: {wikilink}";
            }

            var wiki = GetWiki(word);
            if (!string.IsNullOrEmpty(wiki?.error?.code))
            {
                var sb1 = new StringBuilder();
                sb1.AppendLine("灰机wikiApi出错");
                sb1.AppendLine($"错误代码: {wiki?.error?.code}");
                sb1.AppendLine($"错误描述: {wiki?.error?.info}");
                return sb1.ToString().Trim();
            }
            var words = wiki.query.search.Select(s => s.title).Where(w => w.Format() == word.Format()).ToArray();
            if (words.Any())
            {
                return $"为指挥官献上[{word}]的链接: {wikilink + Uri.EscapeUriString(words.First())}";
            }
            var sb = new StringBuilder();
            sb.AppendLine($"Wiki页面 {word} 不存在.");
            var similarlist = wiki.query.search.Select(s => s.title).Take(3).ToArray();
            if (similarlist.Any())
            {
                sb.AppendLine("相似内容:（可复制下面来搜索)");
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
