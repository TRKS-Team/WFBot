using System;
using System.Collections.Generic;

namespace TRKS.WF.QQBot
{
    [Configuration("WFConfig")]
    internal class Config : Configuration<Config>
    {
        public List<string> WFGroupList = new List<string>();

        public List<string> InvationRewardList = new List<string>();

        public string Code;

        public string QQ;

        public bool AcceptInvitation;

        public bool AcceptJoiningRequest;

        public string ClientId;

        public string ClientSecret;

        public string AcessToken;

        public DateTime Last_update;

        public string GithubOAuthKey;
    }
}