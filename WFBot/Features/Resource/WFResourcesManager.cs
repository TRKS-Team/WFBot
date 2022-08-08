using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GammaLibrary.Extensions;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Features.Resource
{
    public static class WFResourcesManager
    {
        public static ConcurrentDictionary<string, List<IWFResource>> WFResourceDic = new ConcurrentDictionary<string, List<IWFResource>>();
        public static List<GitHubInfo> WFResourceGitHubInfos = new List<GitHubInfo>();
        public static void AddInfo(string name, string kraber, string category, bool iskraber)
        {
            WFResourceGitHubInfos.Add(new GitHubInfo(name, kraber, category, iskraber));
        }
    }

    public class GitHubInfo
    {
        public GitHubInfo(string name, string kraber, string category, bool iskraber)
        {
            LastUpdated = DateTime.Now;
            Name = name;
            Category = category;
            Kraber = kraber;
            IsKraber = iskraber;
            SHA = WFResources.GetSHA(this);
        }

        public string Name { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Category { get; set; }
        public string SHA { get; set; }
        public string Kraber { get; set; }
        public bool IsKraber { get; set; }
    }

    [Configuration("GitHubInfos")]
    public class GitHubInfos : Configuration<GitHubInfos>
    {
        public List<GitHubInfo> Infos = new List<GitHubInfo>();
        protected override void AfterUpdate()
        {
            if (Infos.Select(i => i.Name).Except(WFResourcesManager.WFResourceGitHubInfos.Select(i => i.Name)).Any() || Infos.All(i => i.Kraber.IsNullOrEmpty()))
            {
                Infos = WFResourcesManager.WFResourceGitHubInfos;
            }
        }
    }

}
