using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WFBot.Utils;

namespace WFBot.Features.Resource
{
    public static class WFResourcesManager
    {
        public static ConcurrentDictionary<string, List<IWFResource>> WFResourceDic = new ConcurrentDictionary<string, List<IWFResource>>();
        public static List<GitHubInfo> WFResourceGitHubInfos = new List<GitHubInfo>();
    }

    public class GitHubInfo
    {
        public string Name { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Category { get; set; }
        public string SHA { get; set; }
    }
    [Configuration("GitHubInfos")]
    public class GitHubInfos : Configuration<GitHubInfos>
    {
        public List<GitHubInfo> Infos = new List<GitHubInfo>();

        public GitHubInfos()
        {
            if (!Infos.Any())
            {
                Infos = WFResourcesManager.WFResourceGitHubInfos;
            }
        }
    }    
}
