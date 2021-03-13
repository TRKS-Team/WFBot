using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Features.Resource
{
    public static class SlangManager
    {
        public static WFResource<List<SlangItem>> SlangResource;
        public static List<SlangItem> LocalSlang => SlangConfig.Instance.AllSlang;

        public static IEnumerable<SlangItem> AllSlang => SlangResource.Value.Concat(LocalSlang);


        public static async Task UpdateOnline()
        {
            if (SlangResource == null)
            {
                SlangResource = WFResource<List<SlangItem>>.Create(
                    "http://github.cdn.therealkamisama.top/https://github.com/TRKS-Team/WFBotSlang/blob/main/slang.json", 
                    category: nameof(WFTranslator),
                    resourceLoader: ResourceLoaders<List<SlangItem>>.JsonDotNetLoader);
                await SlangResource.WaitForInited();
                return;
            }

            await SlangResource.Reload();
        }

        
    }

    [Configuration("slang")]
    public class SlangConfig : Configuration<SlangConfig>
    {
        public List<SlangItem> AllSlang { get; set; } = new List<SlangItem>();
    }

    public class SlangItem
    {
        public string Source { get; }
        public string[] Slang { get; }

        public SlangItem(string source, string[] slang) => 
            (Source, Slang) = (source, slang);
    }
}
