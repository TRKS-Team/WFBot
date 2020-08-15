using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using WFBot.Connector;
using WFBot.Events;
using WFBot.Features.Other;
using WFBot.Features.Timers;
using WFBot.Features.Timers.Base;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot
{
    static class Program
    {
        public static void Main()
        {
            var wfbot = new WFBotCore();
            WFBotCore.Instance = wfbot;
            try
            {
                wfbot.Init();
            }
            catch (Exception e)
            {
                Console.WriteLine("WFBot 在初始化中遇到了问题.");
                Console.WriteLine($"{e.GetType().FullName}: {e.Message}");
                Trace.WriteLine(e);
                Console.WriteLine("按任意键继续.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("WFBot fully loaded.");
            Thread.CurrentThread.Join();
        }
    }

    public class WFBotCore
    {
        public WFNotificationHandler NotificationHandler { get; private set; }
        public static WFBotCore Instance { get; internal set; }
        private MessageReceivedEvent messageReceivedEvent;

        public void OnMessage(string groupID, string userID, string message)
        {
            if (!Inited) return;
            messageReceivedEvent.ProcessGroupMessage(groupID.ToGroupNumber(), userID.ToHumanQQNumber(), message);
        }

        internal void Init()
        {
            InitLogger();
            ConnectorManager.LoadConnector();
            Plugins.Load();
            WFResource.InitWFResource();
            messageReceivedEvent = new MessageReceivedEvent();
            NotificationHandler = new WFNotificationHandler();
            InitTimer();
            Inited = true;
        }

        public bool Inited { get; private set; }

        private void InitLogger()
        {
            Directory.CreateDirectory("WFBotLogs");
            var listener = new TextWriterTraceListener(File.Open(Path.Combine($"WFBotLogs", "WFBot-{DateTime.Now:yy-MM-dd_HH.mm.ss}.log"),
                    FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            { TraceOutputOptions = TraceOptions.Timestamp };
            Trace.Listeners.Add(listener);
            Trace.AutoFlush = true;
            Trace.WriteLine($"WFBot start.", "WFBot Core");
        }


        private List<WFBotTimer> timers = new List<WFBotTimer>();
        private void InitTimer()
        {
            // TODO 这里需要测试这个lexion能不能正常工作
            //AddTimer<LexionTimer>();
            AddTimer<NotificationTimer>();

            void AddTimer<T>() where T : WFBotTimer
            {
                timers.Add(Activator.CreateInstance<T>());
            }
        }



    }
}

