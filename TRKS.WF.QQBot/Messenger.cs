using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newbe.Mahua;

namespace TRKS.WF.QQBot
{
    public static class Messenger
    {
        public static void SendPrivate(string qq, string content)
        {
            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
            {
                var api = robotSession.MahuaApi;
                api.SendPrivateMessage(qq, content);
            }
        }

        public static void SendGroup(string qq, string content)
        {
            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
            {
                var api = robotSession.MahuaApi;
                api.SendGroupMessage(qq, content);
            }
        }

        /* 当麻理解不了下面的代码
        public static void SendToGroup(this string content, string qq)
        {
            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
            {
                var api = robotSession.MahuaApi;
                api.SendGroupMessage(qq, content);
            }
        }

        public static void SendToPrivate(this string content, string qq)
        {
            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
            {
                var api = robotSession.MahuaApi;
                api.SendPrivateMessage(qq, content);
            }
        }
        */
    }
}
