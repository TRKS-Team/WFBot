using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFBot.Features.Utils;
using WFBot.TextCommandCore;
using WFBot.Utils;

namespace WFBot.Features.Commands
{

    public partial class CommandsHandler
    {
        [Matchers("wiki")]
        [CombineParams]
        async Task<string> Wiki(string word = "wiki")
        {
            if (word == "wiki")
            {
                return $"为指挥官献上wiki的链接: {wikilink}";
            }

            var wiki = await GetWiki(word);
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
                return $"为指挥官献上[{word}]的链接: {wikilink + Uri.EscapeUriString(words.First()).Replace("'", "%27")}";
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
            /*return
                "灰机wiki的warframe分区由于不规范爬虫导致暂时隔离, warframe区全站不可访问, 具体信息请看: https://www.huijiwiki.com/wiki/Warframe%E4%B8%AD%E6%96%87%E7%BB%B4%E5%9F%BA:403";*/
            // 这简直就是官方吞mod最形象的解释
        }

        private const string wikilink = "https://warframe.huijiwiki.com/wiki/";

        
        Task<Wiki> GetWiki(string word)
        {
            return WebHelper.DownloadJsonAsync<Wiki>(
                $"http://warframe.huijiwiki.com/api.php?action=query&format=json&formatversion=2&list=search&srsearch={word}");
        }
    }
}
