using System;
using System.Collections.Generic;
using System.Diagnostics;
using GammaLibrary.Extensions;
using Newtonsoft.Json;
using WFBot.Features.Other;
using WFBot.Orichalt;
using WFBot.Utils;
using WFBot.Windows;

namespace WFBot
{
    [Configuration("WFConfig")]
    public class Config : Configuration<Config>
    {
        public string ClientGUID = Guid.NewGuid().ToString("D");
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

        public bool IsSlashRequired = false;

        public int CallperMinute = 0;

        public Platform Platform = Platform.PC;

        public bool IsThirdPartyWM = false;

        public bool IsAlertRequiredRareItem = true;

        public bool AutoUpdate = true;

        public bool UpdateLexion = true;

        public string localsha;

        public int WMSearchCount = 3;

        public int WFASearchCount = 5;

        public bool IsPublicBot = false;// 大聪明用户, 别改这个参数.

        public DateTime SendSentientOutpostTime = DateTime.Now;

        public bool NotifyBeforeResult = true;

        public Dictionary<string, string> CustomReplies = new Dictionary<string, string>();

        public bool UseTelemetry = true;
        public MessagePlatform Miguel_Platform = MessagePlatform.Unknown;

        protected override void AfterUpdate()
        {
            // 这里我写的好丑
            // It's not stupid if it works
            if (Code.IsNullOrWhiteSpace() || Code.Contains(' '))
            {
                Trace.WriteLine("警告: 口令中包含空格, 可能会影响带口令指令的使用.");
            }

            if (GitHubOAuthKey.NotNullNorWhiteSpace() && GitHubOAuthKey.Length != 40)
            {
                Trace.WriteLine("警告: GitHubOauthKey 格式错误, 如果你不知道这是什么请不要乱填.");
            }
        }
    }

}