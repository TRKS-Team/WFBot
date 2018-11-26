using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Settings;

namespace TRKS.WF.QQBot.MahuaEvents
{
    /// <summary>
    /// 群消息接收事件
    /// </summary>
    public class GroupMessageReceivedMahuaEvent1
        : IGroupMessageReceivedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;
        private static readonly WFNotificationHandler _wFAlert = new WFNotificationHandler();
        private static readonly WFStatus _wFStatus = new WFStatus();
        private static readonly WMSearcher _wmSearcher = new WMSearcher();

        public GroupMessageReceivedMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessGroupMessage(GroupMessageReceivedContext context)
        {
            try
            {
                var message = context.Message;
                if (message.StartsWith("/"))
                {
                    var command = message.Substring(1);
                    var syndicates = new List<string>() {"赏金", "平原赏金", "地球赏金", "金星赏金", "金星平原赏金", "地球平原赏金"};
                    var fissures = new List<string>() {"裂隙", "裂缝", "虚空裂隙", "查询裂缝", "查询裂隙"};
                    if (syndicates.Where(ostron => command.StartsWith(ostron)).Any())
                    {
                        var indexString = command.Substring(syndicates.Where(ostron => command.StartsWith(ostron)).First().Length);
                        if (indexString.IsNumber())
                        {
                            var index = Int32.Parse(indexString);
                            if (index <= 5 && 0 < index)
                            {
                                _wFStatus.SendSyndicateMissions(context.FromGroup, index);
                            }
                            else
                            {
                                _wFStatus.SendSyndicateMissions(context.FromGroup, 1);
                            }

                        }
                        else
                        {
                            Messenger.SendGroup(context.FromGroup, "需要参数,例子:/赏金 4 将会返回赏金四.");
                        }
                    }

                    if (fissures.Where(fissure => command.StartsWith(fissure)).Any())
                    {
                        var words = command.Split(' ').ToList();
                        if (words.Count >= 2)
                        {
                            words.RemoveAt(0);
                            _wFStatus.SendFissures(context.FromGroup, words);
                        }
                        else
                        {
                            Messenger.SendGroup(context.FromGroup, $"需要参数,列如: /查询裂隙 古纪(第一个为主条件) 歼灭(之后可添加多个小条件).");
                        }

                    }
                    if (command.StartsWith("查询"))
                    {
                        var item = command.Substring(3).Replace(" ", "").ToLower();
                        _wmSearcher.SendWMInfo(item, context.FromGroup);
                    }
                    switch (command)
                    {
                        case "警报":
                            _wFAlert.SendAllAlerts(context.FromGroup);
                            break;
                        case "平野":
                        case "夜灵平野":
                        case "平原":
                        case "夜灵平原":
                            _wFStatus.SendCetusCycle(context.FromGroup);
                            break;
                        case "入侵":
                            _wFAlert.SendAllInvasions(context.FromGroup);
                            break;
                        case "突击":
                            _wFStatus.SendSortie(context.FromGroup);
                            break;
                        case "奸商":
                        case "虚空商人":
                        case "商人":
                            _wFStatus.SendVoidTrader(context.FromGroup);
                            break;
                        case "help":
                        case "帮助":
                        case "功能":
                            Messenger.SendGroup(context.FromGroup, @"欢迎查看破机器人的帮助文档,如有任何bug和崩溃请多多谅解.
作者:TheRealKamisama 开源地址:https://github.com/TheRealKamisama/WFBot
!!!符号说明:[]符号内的所有字符为命令,{}符号内里的所有说明为按键,<>符号内的是命令所需参数的名称,()符号内的所有说明为功能或参数的注释.
功能1:警报 可使用[/警报]来直接查询所有警报.
      新警报也会自动发送到所有启用了通知功能的群.
功能2:入侵 可使用[/入侵]来查询所有入侵.
      新入侵也会自动发送到所有启用了通知功能的群.
功能3:突击 可使用[/突击]来查询所有突击.
     突击的奖励池为一般奖励池.
功能4:平原时间 可使用[/平原(此处写了一个小词库,列如平野也可识别)]来查询平原目前的时间.
功能5:虚空商人信息 可使用[/虚空商人(或者你输入奸商也可以)]来查询奸商的状态.
     !注意:如果虚空商人已经抵达将会输出所有的商品和价格,长度较长.
功能6:WarframeMarket 可使用[/查询{空格}<物品名称>(不区分大小写,无需空格.)]
     !注意:物品名称必须标准,比如 总图 将无法识别 须输入 蓝图.
功能7:赏金 可使用[/赏金(同义词也可){空格}<赏金数>(比如赏金一就是1)]来查询地球和金星的单一赏金任务.
     !注意:必须需要参数.
功能8:裂隙 可使用[/裂隙{空格}<关键词>(比如 前纪,歼灭)]来查询所有和关键词有关的裂隙.
其他功能待定(查询遗物的功能因为字典更换被毙了.)
用于管理的命令均为私聊机器人:
用于启用群通知:[添加群{空格}<口令>{空格}<群号>]
用于禁用群通知:[删除群{空格}<口令>{空格}<群号>]");
                            break;
                    }
                }


            }
            catch (Exception e)
            {
                Messenger.SendPrivate("1141946313", e.ToString());
            }

        }
    }
}
