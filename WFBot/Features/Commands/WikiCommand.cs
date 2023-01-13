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
        /*[Matchers("wiki", "wk")]
        [CombineParams]
        async Task<string> Wiki(string word = "wiki")
        {
            if (word == "wiki")
            {
                return WFFormatter.GetWikiLink();
            }

            var wiki = await GetWiki(word);
            return WFFormatter.FormatWikiCommand(word, wiki);
            /*return
                "灰机wiki的warframe分区由于不规范爬虫导致暂时隔离, warframe区全站不可访问, 具体信息请看: https://www.huijiwiki.com/wiki/Warframe%E4%B8%AD%E6%96%87%E7%BB%B4%E5%9F%BA:403";*/
            // 这简直就是官方吞mod最形象的解释
        // }



        /*private const string wikilink = "https://warframe.huijiwiki.com/wiki/";


        Task<Wiki> GetWiki(string word)
        {
            return WebHelper.DownloadJsonAsync<Wiki>(
                $"http://warframe.huijiwiki.com/api.php?action=query&format=json&formatversion=2&list=search&srsearch={word}");
        }*/
    }
}
