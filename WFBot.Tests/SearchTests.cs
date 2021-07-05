using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using GammaLibrary.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WFBot.Features.Common;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Tests
{
    [TestClass]
    public class SearchTests
    {

        [TestInitialize]
        public void Init()
        {
            AsyncContext.SetMessageSender(new MessageSender());
            WFBotCore.UseTestConnector = true;
            new WFBotCore().Init().Wait();
        }

        [TestMethod]
        public void TestSearch()
        {
            return;
            Assert.IsFalse(WMSearcher.Search("凯旋将军破坏者", out _));
            Assert.IsFalse(WMSearcher.Search("valkyrp", out _));
            Assert.IsFalse(WMSearcher.Search("valkyrp总图", out _));
            Assert.IsFalse(WMSearcher.Search("赫利俄斯p头", out _));
            Assert.IsFalse(WMSearcher.Search("valkyrp头", out _));
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
