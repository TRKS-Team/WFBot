using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TRKS.WF.QQBot
{
    [Configuration("WFConfig")]
    internal class Config : Configuration<Config>
    {
        public List<string> WFGroupList = new List<string>();

        public List<string> InvationRewardList = new List<string>();

        public string Code = "*******";

        public string QQ;

        public bool AcceptInvitation;

        public bool AcceptJoiningRequest;

        public string ClientId;

        public string ClientSecret;

        public string AcessToken;

        public DateTime Last_update;

        [JsonProperty("Git" + "hubOAuthKey")]
        public string GitHubOAuthKey;

        public bool IsSlashRequired = true;

        public int CallperMinute = 0;

        public Settings.Platform Platform = Settings.Platform.PC;

        public bool IsThirdPartyWM = false;

        public bool IsAlertRequiredRareItem = true;

        public bool AutoUpdate = true;

        public bool UpdateLexion = true;

        public string localsha;

        public int WMSearchCount = 3;

        public int WFASearchCount = 5;

        public bool IsPublicBot = false;// 大聪明用户, 别改这个参数.
    }

    [Configuration("CoquettishConfig")]
    internal class CoquettishConfig : Configuration<CoquettishConfig>
    {
        public List<CoquettishWord> CoquettishWords = new List<CoquettishWord>();
    }

    public class CoquettishWord
    {
        public CoquettishWord(string word, string result)
        {
            this.word = word;
            this.result = result;
        }

        public string word { get;}
        public string result { get;}
    }


}