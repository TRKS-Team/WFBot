using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using GammaLibrary.Extensions;
using Humanizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WFBot.Features.Common;
using WFBot.Features.Events;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Tests
{
    [TestClass]
    public class FunctionTest
    {
        [TestInitialize]
        public void Init()
        {
            AsyncContext.SetMessageSender(new MessageSender());
            WFBotCore.UseTestConnector = true;
            core = new WFBotCore();
            core.Init().Wait();
        }
        WFBotCore core;
        public MessageReceivedEvent GroupMessageRecivedEvent => core.messageReceivedEvent;

        [TestMethod]
        public void TestFunctions()
        {
            InputCommand("/金星赏金");
            InputCommand("/金星赏金 5");
            InputCommand("/地球赏金");
            InputCommand("/地球赏金 5");
            InputCommand("/火卫赏金");
            InputCommand("/火卫赏金 7");
            InputCommand("/裂隙");
            InputCommand("/裂隙 1");
            InputCommand("/遗物 后纪L4"); // 我还有一个, 有没有人陪我开了?
            InputCommand("/翻译 致残突击");
            InputCommand("/赤毒");
            InputCommand("/警报");
            InputCommand("/突击");
            InputCommand("/入侵");
            InputCommand("/仲裁");
            InputCommand("/午夜电波");
            InputCommand("/平原");
            InputCommand("/活动");
            InputCommand("/虚空商人");
            InputCommand("/小小黑");
            InputCommand("/s船");
            InputCommand("/wiki");
            InputCommand("/查询 Valkyr Prime 一套");
            InputCommand("/紫卡 绝路");
            InputCommand("/WFA紫卡 绝路");
            InputCommand("/wiki Valkyr");
            InputCommand("/status");
            InputCommand("/help");
            InputCommand("/查询 瓦尔基里");
            InputCommand("/查询 电男");
            InputCommand("/查询 致残");
            File.AppendAllText("TestResult.log", Environment.NewLine + "Done.");
        }

        private void InputCommand(string msg)
        {
            GroupMessageRecivedEvent.ProcessGroupMessage("0", "0", msg).Wait();
        }
        [TestMethod]
        public void TestWildcards()
        {
            var test = new List<Sale>();
            Assert.IsTrue(WMSearcher.Search("凯旋将军破坏者", ref test)); // 破坏者不加p
            test.Clear();
            Assert.IsTrue(WMSearcher.Search("valkyrp", ref test)); // p自动补全为prime
            test.Clear();
            Assert.IsTrue(WMSearcher.Search("valkyrp总图", ref test)); // 总图替换成蓝图且替换p
            test.Clear();
            Assert.IsTrue(WMSearcher.Search("赫利俄斯p头", ref test)); // 头替换成头部且替换p
            test.Clear();
            Assert.IsTrue(WMSearcher.Search("valkyrp头", ref test)); // 头替换成头部神经光元且替换p
            test.Clear();
            Assert.IsTrue(WMSearcher.Search("vome", ref test)); // 纯英物品
            test.Clear();
            Assert.IsTrue(WMSearcher.Search("hlesptb", ref test)); // 拼音匹配
            test.Clear();
            Assert.IsTrue(WMSearcher.Search("hlesptbsjgy", ref test)); // 头部替换成头部神经光元
            test.Clear();
            Assert.IsTrue(WMSearcher.Search("电男p", ref test)); // 黑话
        }
    }

    class MessageSender : IGroupMessageSender
    {
        public GroupID GroupID { get; }
        const string resultPath = "TestResult.log";
        public void SendMessage(string msg)
        {

            Trace.WriteLine(msg);
            if (File.Exists(resultPath) && File.ReadLines(resultPath).Last() == "Done.") // 哈哈 Trick.
            {
                File.Delete(resultPath);
            }
            File.AppendAllText(resultPath, msg + Environment.NewLine);
        }
    }
}
