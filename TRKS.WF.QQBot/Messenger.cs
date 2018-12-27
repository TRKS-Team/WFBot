using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newbe.Mahua;
using Settings;

namespace TRKS.WF.QQBot
{
    public static class Messenger
    {
        public static void SendDebugInfo(string content)
        {
            if (content.StartsWith("System.Threading.ThreadAbortException")) return;
            
            if (Config.Instance.QQ.IsNumber())
                SendPrivate(Config.Instance.QQ, content);
        }

        public static void SendPrivate(string qq, string content)
        {
            //if (Config.Instance.QQ.IsNumber())
            //{
            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
            {
                var api = robotSession.MahuaApi;
                api.SendPrivateMessage(qq, content);
            }
            //}
        }

        private static Dictionary<string, string> previousMessageDic = new Dictionary<string, string>();
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SendGroup(string qq, string content)
        {
            if (previousMessageDic.ContainsKey(qq) && content == previousMessageDic[qq]) return;
            
            previousMessageDic[qq] = content;

            using (var robotSession = MahuaRobotManager.Instance.CreateSession())
            {
                var api = robotSession.MahuaApi;
                api.SendGroupMessage(qq, content);
            }
            Thread.Sleep(1000); //我真的很生气 为什么傻逼tencent服务器就不能让我好好地发通知 NMSL
        }

        public static void Broadcast(string content)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var group in Config.Instance.WFGroupList)
                {
                    SendGroup(group, content + Environment.NewLine + group);
                    Thread.Sleep(6000); //我真的很生气 为什么傻逼tencent服务器就不能让我好好地发通知 NMSL
                }
            }, TaskCreationOptions.LongRunning);
        }
        public static void SendHelpdoc(string group)
        {
            SendGroup(group, @"欢迎查看机器人唯一指定帮助文档
宣传贴地址:https://warframe.love/thread-230.htm
开源地址:https://github.com/TheRealKamisama/WFBot (具体的使用方法请见此链接.)
本机器人为公益项目,持续维护中.");
            if (File.Exists("data/image/帮助文档.png"))
            {
                SendGroup(group, @"[CQ:image,file=\帮助文档.png]*");
            }
            else
            {
                SendGroup(group, @"欢迎查看破机器人的帮助文档,如有任何bug和崩溃请多多谅解.
作者:TheRealKamisama 开源地址:https://github.com/TheRealKamisama/WFBot
符号说明:[]符号内的所有字符为命令,{}符号内里的所有说明为按键,<>符号内的是命令所需参数的名称,()符号内的所有说明为功能或参数的注释.
功能1:警报 可使用[/警报]来直接查询所有警报.
      新警报也会自动发送到所有启用了通知功能的群.
功能2:入侵 可使用[/入侵]来查询所有入侵.
      新入侵也会自动发送到所有启用了通知功能的群.
功能3:突击 可使用[/突击]来查询所有突击.   
功能4:平原时间 可使用[/平原]来查询地球平原目前的时间和金星平原目前的气温.
功能5:虚空商人信息 可使用[/虚空商人(或者你输入奸商也可以)]来查询奸商的状态.
     如果虚空商人已经抵达将会输出所有的商品和价格,长度较长.
功能6:WarframeMarket 可使用[/查询{空格}<物品名称>(不区分大小写,无需空格.)]
     *物品名称必须标准.*
     *查询一个物品需要后面加一套*
     *查询prime版物品必须加prime后缀*
     *prime不可以缩写成p*
     *查询未开紫卡请输入:手枪未开紫卡*
功能7:赏金 可使用[/赏金(同义词也可){空格}<赏金数>(赏金一是1)]来查询地球和金星的单一赏金任务.
     注意:必须需要参数.
功能8:裂隙 可使用[/裂隙{空格}<关键词>(比如 前纪, 歼灭)]来查询所有和关键词有关的裂隙.
用于管理的命令均为私聊机器人:
    启用群通知:[添加群{空格}<口令>{空格}<群号>]
    禁用群通知:[删除群{空格}<口令>{空格}<群号>]");
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
