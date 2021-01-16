using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using WFBot.Connector;
using WFBot.Events;
using WFBot.Features.Other;
using WFBot.Features.Resource;
using WFBot.Features.Timers;
using WFBot.Features.Timers.Base;
using WFBot.Features.Utils;
using WFBot.Utils;
using WFBot.Windows;

namespace WFBot
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            https://github.com/TRKS-Team/WFBot
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            var wfbot = new WFBotCore();
            WFBotCore.Instance = wfbot;

            var skipPressKey = false;
            foreach (var s in args)
            {
                switch (s)
                {
                    case "--skip-press-any-key":
                        skipPressKey = true;
                        break;
                }
            }

            var sw = Stopwatch.StartNew();
            try
            {
                await wfbot.Init();
            }
            catch (Exception e)
            {
                Console.WriteLine("WFBot 在初始化中遇到了问题.");
                Trace.WriteLine(e);
                if (!skipPressKey)
                {
                    Console.WriteLine("按任意键继续.");
                    Console.ReadKey();
                }
                return;
            }
            
            Messenger.SendDebugInfo($"WFBot 加载完成. 用时 {sw.Elapsed.TotalSeconds:F1}s.");
            while (true)
            {
                var text = Console.ReadLine();
                switch (text.ToLower())
                {
                    case "ui":
                        OpenWFBotSettingsWindow();
                        break;
                    case "exit":
                    case "stop":
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

        internal async Task Init()
        {
            InitLogger();
            var version = GetVersion();
            Trace.WriteLine($"WFBot: 开始初始化. 版本号 {version}");
            Console.Title = $"WFBot {version}";

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Trace.WriteLine($"Task 发生异常: {args.Exception}.");
                args.SetObserved();
            };
            Plugins.Load();
            ConnectorManager.LoadConnector();
            await WFResources.InitWFResource();
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
        }

        static string GetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName().Name;
            var gitVersionInformationType = assembly.GetType("GitVersionInformation");
            var versionField = gitVersionInformationType.GetField("InformationalVersion");
            return versionField.GetValue(null) as string;
        }

        private List<WFBotTimer> timers = new List<WFBotTimer>();
        private void InitTimer()
        {
            AddTimer<LexionTimer>();
            AddTimer<NotificationTimer>();

            void AddTimer<T>() where T : WFBotTimer
            {
                timers.Add(Activator.CreateInstance<T>());
            }
        }



    }
}

