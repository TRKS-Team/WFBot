using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using GammaLibrary.Extensions;
using WFBot.Connector;
using WFBot.Events;
using WFBot.Features.Other;
using WFBot.Features.Timers;
using WFBot.Features.Timers.Base;
using WFBot.Features.Utils;
using WFBot.Utils;
using WFBot.Windows;

namespace WFBot
{
    static class Program
    {
        public static void Main()
        {
            var wfbot = new WFBotCore();
            WFBotCore.Instance = wfbot;

            var sw = Stopwatch.StartNew();
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
            Messenger.SendDebugInfo($"WFBot 加载完成. 用时 {sw.Elapsed.TotalSeconds:F1}s");

            while (true)
            {
                var text = Console.ReadLine();
                switch (text.ToLower())
                {
                    case "ui":
                        OpenWFBotSettingsWindow();
                        break;
                    case "exit":
                        return;
                    default:
                        ConnectorManager.Connector.OnCommandLineInput(text);
                        break;
                }
            }
        }

        private static void OpenWFBotSettingsWindow()
        {
#if WINDOWS_RELEASE
            var thread = new Thread(() =>
            {
                new Settings().ShowDialog();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
#else
            Console.WriteLine("此编译版本不包含UI. 请使用 Windows 编译版本.");
#endif

        }

    }

    public class WFBotCore
    {
        public WFNotificationHandler NotificationHandler { get; private set; }
        public static WFBotCore Instance { get; internal set; }
        private MessageReceivedEvent messageReceivedEvent;
        private PrivateMessageReceivedEvent privateMessageReceivedEvent;

        public void OnGroupMessage(GroupID groupID, UserID userID, string message)
        {
            if (!Inited)
            {
                Trace.WriteLine($"Message ignored due to uninitialized: {groupID} {userID} {message}");
                return;
            }
            messageReceivedEvent.ProcessGroupMessage(groupID, userID, message);
        }

        public void OnFriendMessage(UserID userID, string message)
        {
            if (!Inited) return;
            privateMessageReceivedEvent.ProcessPrivateMessage(userID, message);
        }

        internal void Init()
        {
            InitLogger();
            Plugins.Load();
            ConnectorManager.LoadConnector();
            WFResource.InitWFResource();
            messageReceivedEvent = new MessageReceivedEvent();
            privateMessageReceivedEvent = new PrivateMessageReceivedEvent();
            NotificationHandler = new WFNotificationHandler();
            InitTimer();
            Inited = true;
        }

        public bool Inited { get; private set; }

        private void InitLogger()
        {
            Directory.CreateDirectory("WFBotLogs");
            var fileListener = new TextWriterTraceListener(File.Open(Path.Combine($"WFBotLogs", $"WFBot-{DateTime.Now:yy-MM-dd_HH.mm.ss}.log"),
                    FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            { TraceOutputOptions = TraceOptions.Timestamp };
            Trace.Listeners.Add(fileListener);
            Trace.Listeners.Add(new ConsoleTraceListener());
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

