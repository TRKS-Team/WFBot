using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using WFBot.Connector;
using WFBot.MahuaEvents;
using WFBot.Orichalt;
using WFBot.Utils;
using Timer = System.Timers.Timer;

namespace WFBot.Features.Utils
{

    public static class Messenger
    {

        static Messenger()
        {
            // 大家都知道你很蠢啦
        }

        public static void SendDebugInfo(string content)
        {
            if (Config.Instance.QQ.IsNumber())
                MiguelNetwork.SendDebugInfo(content);
            Trace.WriteLine($"{content}", "Message");
        }

        public static void SendPrivate(UserID qq, string content)
        {
            ConnectorManager.Connector.SendPrivateMessage(qq, content);
            // todo
        }

        static readonly Dictionary<GroupID, string> previousMessageDic = new Dictionary<GroupID, string>();
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SendGroup(GroupID g, string content)
        {
            var qq = g.ID;
            // 避免重复发送同一条消息
            if (previousMessageDic.ContainsKey(qq) && content == previousMessageDic[qq]) return;
            previousMessageDic[qq] = content;

            MiguelNetwork.SendToGroup(g, content);
            // IncreaseCallCounts(g);
            //Thread.Sleep(1000); //我真的很生气 为什么傻逼tencent服务器就不能让我好好地发通知 NMSL
        }

        /*public static void Broadcast(string content)
        {
            Task.Factory.StartNew(() =>
            {
                var count = 0;
                foreach (var group in Config.Instance.WFGroupList)
                {
                    if (count > 20 && content.StartsWith("机器人开始了自动更新")) return;

                    var sb = new StringBuilder();
                    sb.Append("[WFBot通知] ");
                    sb.AppendLine(content);
                    if (count > 10) sb.AppendLine($"发送次序: {count}(与真实延迟了{7 * count}秒)");
                    // sb.AppendLine($"如果想要获取更好的体验,请自行部署.");
                    sb.ToString().Trim().SendToGroup(group);
                    count++;
                    Thread.Sleep(7000); //我真的很生气 为什么傻逼tencent服务器就不能让我好好地发通知 NMSL
                }
            }, TaskCreationOptions.LongRunning);
        }
        

        /* 当麻理解不了下面的代码 */
        // 现在可以了
        public static void SendToGroup(this string content, GroupID qq)
        {
            SendGroup(qq, content);
        }

        public static void SendToPrivate(this string content, UserID qq)
        {
            SendPrivate(qq, content);
        }


        /*
        public static void SuperBroadcast(string content)
        {
            var groups = GetGroups().Select(g => g.Group);
            Task.Factory.StartNew(() =>
            {
                foreach (var group in groups)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(content);
                    SendGroup(group.ToGroupNumber(), sb.ToString().Trim());
                    Thread.Sleep(7000); //我真的很生气 为什么傻逼tencent服务器就不能让我好好地发通知 NMSL
                }
            }, TaskCreationOptions.LongRunning);
        }
        */
    }

    public struct GroupID
    {
        public long ID { get; }

        public GroupID(long id)
        {
            ID = id;
        }

        public static implicit operator long(GroupID id)
        {
            return id.ID;
        }
        

        public static implicit operator string(GroupID id)
        {
            return id.ToString();
        }

        public static implicit operator GroupID(long id)
        {
            return new GroupID((uint) id);
        }

        public static implicit operator GroupID(string id)
        {
            return new GroupID(id.ToUInt());
        }

        public override string ToString()
        {
            return ID.ToString();
        }
    }

    public struct UserID
    {
        public long ID { get; }

        public UserID(long id)
        {
            ID = id;
        }

        public static implicit operator long(UserID id)
        {
            return id.ID;
        }
        
        public static implicit operator string(UserID id)
        {
            return id.ToString();
        }

        public static implicit operator UserID(long id)
        {
            return new UserID((uint)id);
        }

        public static implicit operator UserID(string id)
        {
            return new UserID(id.ToUInt());
        }

        public override string ToString()
        {
            return ID.ToString();
        }
    }
}
