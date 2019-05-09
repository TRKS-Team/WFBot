using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRKS.WF.QQBot
{

    private WFApi translateApi => WFResource.WFApi;
    public String wikiSearch(String word)
    {
        String searchword = "";
        if (word != "")
        {


            if (word.Contains("p"))
            {
                searchword = word.Replace("p", " PRIME");
            }
            else
            {
                searchword = word;
            }
            foreach (var search in translateApi.Dict)
            {
                var searchZh = search.Zh;
                if (searchZh.Contains(searchword) && searchword != "")
                {
                    return "为指挥找到[" + word + "]的wiki链接:https://warframe.huijiwiki.com/wiki/" + search.En.Replace(" ", "_");
                }
            }
            return "很抱歉，未找到[" + word + "]的wiki页面所以，为指挥官献上wiki链接: https://warframe.huijiwiki.com/wiki/";
        }
        else
        {
            return "为指挥官献上wiki的链接:https://warframe.huijiwiki.com/wiki/";
        }
    }
}
