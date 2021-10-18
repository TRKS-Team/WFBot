using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
            var sb = new StringBuilder();
            GroupMessageRecivedEvent.ProcessGroupMessage("0","0","/金星赏金").Wait();
        }
        [TestMethod]
        public void TestWildcards()
        {
            Assert.IsTrue(WMSearcher.Search("凯旋将军破坏者", out _)); // 破坏者不加p
            Assert.IsTrue(WMSearcher.Search("valkyrp", out _)); // p自动补全为prime
            Assert.IsTrue(WMSearcher.Search("valkyrp总图", out _)); // 总图替换成蓝图且替换p
            Assert.IsTrue(WMSearcher.Search("赫利俄斯p头", out _)); // 头替换成头部且替换p
            Assert.IsTrue(WMSearcher.Search("valkyrp头", out _)); // 头替换成头部神经光元且替换p
            Assert.IsTrue(WMSearcher.Search("vome", out _)); // 纯英物品
        }
    }

    class MessageSender : IGroupMessageSender
    {
        public GroupID GroupID { get; }
        public void SendMessage(string msg)
        {
            Trace.WriteLine(msg);
        }
    }
}
