using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using TextCommandCore;
using TRKS.WF.QQBot;
using TRKS.WF.QQBot.MahuaEvents;
// ReSharper disable LocalizableElement

namespace WFBotTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = File.Open("logit.log", FileMode.Create, FileAccess.Write, FileShare.ReadWrite).CreateStreamWriter();
            Messenger.DebugAlternateHandler = s => { file.WriteLine(s); };
            Messenger.MessageAlternateHandler = s => { file.WriteLine(s); };
            GroupMessageReceivedMahuaEvent1._WfNotificationHandler.Update();

            Do("金星赏金");
            Do("地球赏金");
            Do("查询 加速赋能");
            Do("查询 加速赋能 -QR");
            Do("紫卡 关刀");
            Do("翻译 关刀");
            Do("遗物 中纪V2");
            Do("平原时间");
            Do("入侵");
            Do("突击");
            Do("奸商");
            Do("活动");
            Do("裂隙");
            Do("小小黑");
            Do("状态");
            Do("午夜电波");

            file.Flush();
            void Do(string msg)
            {
                var sw = Stopwatch.StartNew();
                file.WriteLine($"Command: {msg}");
                file.WriteLine("---------");
                new GroupMessageHandler(new HumanQQNumber(""), new GroupNumber(""), msg).ProcessCommandInput();
                file.WriteLine($"--------- Elapsed time: {sw.Elapsed.Milliseconds:F1}ms");
                file.WriteLine();
            }
        }
    }
}
