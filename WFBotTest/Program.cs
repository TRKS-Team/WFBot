using System;
using System.Diagnostics;
using TextCommandCore;
using WFBot.Events;
using WFBot.Features.Utils;

// ReSharper disable LocalizableElement

namespace WFBotTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //var file = File.Open("logit.log", FileMode.Create, FileAccess.Write, FileShare.ReadWrite).CreateStreamWriter();
            Trace.Listeners.Add(new ConsoleTraceListener());
            MessengerHandlers.DebugAlternateHandler = s => { Console.WriteLine(s); };
            MessengerHandlers.MessageAlternateHandler = s => { Console.WriteLine(s); };
            WFResource.InitWFResource();
            //GroupMessageReceivedMahuaEvent1._WfNotificationHandler.Update();
            Do("紫卡 瘟疫克里帕丝");
            Do("s船");
            /*Do("赤毒");
            Do("仲裁");
            Do("wiki test");
            Do("紫卡 绝路");
            Do("紫卡 合成燃气炮");*/

            //Do("金星赏金");
            

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
            /*
            Do("查询 valkyr p 头");
            Do("查询 valkyr prime 头");
            Do("查询 valkyr prime 头部");
            Do("查询 valkyr prime 头部神");
            Do("查询 valkyr prime 头部神经");
            Do("查询 valkyr prime 头部神经光");
            Do("查询 valkyr prime 头部神经光元");
            Do("查询 龙甲 头");
            Do("查询 龙甲 头部");
            Do("查询 龙甲 头部神");
            Do("查询 龙甲 头部神经");
            Do("查询 龙甲 头部神经光");
            Do("查询 龙甲 头部神经光元");
            */
            //            Do("查询 龙");
            //            Do("紫卡 瘟疫");
            //            Do("wiki");
            //            Do("wiki 川流不息prime");
            //            Do("wiki 川流不");
            //            Do("wiki Primed");

            //file.Flush();
            Console.WriteLine("fine");
            Console.ReadKey();
            void Do(string msg)
            {
                var sw = Stopwatch.StartNew();
                Console.WriteLine($"Command: {msg}");
                Console.WriteLine("---------");
                //new MessageReceivedEvent().ProcessGroupMessage(new GroupMessageReceivedContext{FromAnonymous = "", FromGroup = "", FromQq = "", Message = ""});
                // 如果你在测试Callperminute的话 返回的数据可能会少一 这正常 因为命令因为↑这行代码多执行了一次
                // 并且Calldic里的数据不会因为Do而减少
                new GroupMessageHandler(new UserID(""), new GroupID(""), msg).ProcessCommandInput();
                Console.WriteLine($"--------- Elapsed time: {sw.Elapsed.Milliseconds:F1}ms");
                Console.WriteLine();
            }
        }
    }
}
