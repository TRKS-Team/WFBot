using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using GammaLibrary.Extensions;
using InternetTime;
using TextCommandCore;
using WFBot.Connector;
using WFBot.Events;
using WFBot.Features.Events;
using WFBot.Features.Other;
using WFBot.Features.Resource;
using WFBot.Features.Timers;
using WFBot.Features.Timers.Base;
using WFBot.Features.Utils;
using WFBot.Utils;
using WFBot.Windows;

#pragma warning disable 164

namespace WFBot
{
    public static class Program
    {
        internal static bool NoLog { get; private set; } = false;
        internal static bool DontThrowIfResourceUnableToLoad { get; private set; } = false;
        internal static bool UseConfigFolder { get; private set; }

        public static async Task Main(string[] args)
        {
        https://github.com/TRKS-Team/WFBot
            var skipPressKey = false;
            var setCurrentFolder = false;

            foreach (var s in args)
            {
                switch (s)
                {
                    case "--skip-press-any-key":
                        skipPressKey = true;
                        break;
                    case "--set-current-folder-to-executable":
                        setCurrentFolder = true;
                        break;
                    case "--no-logs":
                        NoLog = true;
                        break;
                    case "--dont-throw-if-resource-unable-to-load":
                        DontThrowIfResourceUnableToLoad = true;
                        break;
                    case "--use-config-folder":
                        Directory.CreateDirectory("WFBotConfigs");
                        UseConfigFolder = true;
                        break;
                }
            }

            if (setCurrentFolder)
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            }

            var wfbot = new WFBotCore();
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

            await wfbot.Run();
        }
    }
    public sealed class WFBotCore
    {
        public WFBotCore()
        {
            Instance = this;
        }

        public WFNotificationHandler NotificationHandler { get; private set; }
        public static WFBotCore Instance { get; internal set; }
        private MessageReceivedEvent messageReceivedEvent;
        private PrivateMessageReceivedEvent privateMessageReceivedEvent;
        public static string Version { get; }
        public static bool IsOfficial { get; }
        public static bool IsShuttingDown { get; private set; }

        static WFBotCore()
        {
            Version = GetVersion();
            IsOfficial = Version.Split('+').Last() == "official";

            static string GetVersion()
            {
                var assembly = Assembly.GetExecutingAssembly();
                var gitVersionInformationType = assembly.GetType("GitVersionInformation");
                var versionField = gitVersionInformationType?.GetField("InformationalVersion");
                if (versionField is null) return "unofficial";

                return versionField.GetValue(null) as string;
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

        public async Task Run()
        {
            while (true)
            {
                var text = await Console.In.ReadLineAsync();
                if (text == null) continue;

                switch (text.ToLower())
                {
                    case "ui":
                        OpenWFBotSettingsWindow();
                        break;
                    case "exit":
                    case "stop":
                        Shutdown();
                        return;
                    default:
                        if (!(new CustomCommandMatcherHandler(text.TrimStart('/'))).ProcessCommandInput().Result.matched)
                        {
                            ConnectorManager.Connector.OnCommandLineInput(text);
                        }
                        break;
                }
            }
        }

        public void RequestUpdate()
        {
            if (IsShuttingDown) return;
            ShutdownInternal();
            const int UpdateCode = 0xDEAD;
            Environment.Exit(UpdateCode);
        }

        public void Shutdown()
        {
            if (IsShuttingDown) return;
            ShutdownInternal();
            Environment.Exit(0);
        }

        static void ShutdownInternal()
        {
            if (IsShuttingDown) return;
            Trace.WriteLine("WFBot 正在停止..");
            IsShuttingDown = true;
            CheckResourceLock();

            while (WFBotResourceLock.AnyLockAcquired)
            {
                Thread.Sleep(500);
            }
        }

        static void CheckResourceLock()
        {
            var locks = WFBotResourceLock.AllLocks;
            if (!locks.IsEmpty)
            {
                Console.WriteLine(
                    $"当前有资源锁正在被占用: {WFBotResourceLock.AllLocks.Select(l => l.ToString()).Connect(", ", "[", "]")}. \n");
                if (locks.Any(l => l.LockType == ResourceLockTypes.Essential))
                {
                    Console.WriteLine($"强行退出可能会造成一些大问题.");
                }
                else
                {
                    Console.WriteLine("如果等待时间太长, 可以尝试直接退出.");
                }
            }
        }

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

        bool _requestedCtrlCShutdown;

        public async Task Init()
        {
            var sw = Stopwatch.StartNew();
            // ------------------------------------------------------------------
            // 配置 Logger
            InitLogger();

            // 设置版本号
            var version = Version;
            Trace.WriteLine($"WFBot 开始初始化. 版本号 {version}");
            if (IsOfficial)
            {
                Trace.WriteLine("你正在使用官方编译版本. "); // 
            }

            Console.Title = $"WFBot {version}";

            // 设置 Ctrl C 处理
            Console.CancelKeyPress += (sender, args) =>
            {
                if (_requestedCtrlCShutdown) return;
                if (!Inited)
                {
                    Console.WriteLine("WFBot 还在初始化. 再按一次 Ctrl+C 可以强行停止, 但可能会造成一些问题.");
                    args.Cancel = true;
                    _requestedCtrlCShutdown = true;
                    return;
                }
                args.Cancel = true;
                _requestedCtrlCShutdown = true;

                Console.WriteLine("正在停止, 强行停止请再按一次 Ctrl+C.");
                Task.Run(() => Shutdown());
                //Shutdown();
            };

            // Task 异常处理
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Trace.WriteLine($"Task 发生异常: {args.Exception}.");
                args.SetObserved();
            };

            Trace.WriteLine("加载插件...");
            Plugins.Load();

            Trace.WriteLine("加载配置文件...");
            Config.Update();

            Trace.WriteLine("加载 Connector...");
            ConnectorManager.LoadConnector();

            Trace.WriteLine("加载资源...");
            await WFResources.InitWFResource();
            messageReceivedEvent = new MessageReceivedEvent();
            privateMessageReceivedEvent = new PrivateMessageReceivedEvent();
            NotificationHandler = new WFNotificationHandler();

            // 加载自定义命令处理器
            Trace.WriteLine("加载自定义命令处理器...");
            CustomCommandMatcherHandler.InitCustomCommandHandler();

            // 检查时间...
            Task.Run(() => CheckTime());

            // 初始化定时器
            Trace.WriteLine("初始化定时器...");
            InitTimer();

            // ------------------------------------------------------------------
            // 完成
            Inited = true;
            if (_requestedCtrlCShutdown)
            {
                Task.Run(() => Shutdown());
            }

            _requestedCtrlCShutdown = false;
            Messenger.SendDebugInfo($"WFBot 加载完成. 用时 {sw.Elapsed.TotalSeconds:F1}s.");
        }

        void CheckTime()
        {
            try
            {
                if (TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Hours != 8)
                {
                    var msg = "**************警告: 你的系统时区不为 UTC+8, 会造成任务通知不精确.";
                    Messenger.SendDebugInfo(msg);
                }
                var sntpClient = new SNTPClient("ntp.aliyun.com");
                sntpClient.Connect(false);
                var timeSpan = TimeSpan.FromMilliseconds(sntpClient.LocalClockOffset);
                if (timeSpan.TotalMinutes > 1)
                {
                    var msg = $"*************警告: 你的系统时间与世界时间相差了1分钟以上, 具体来说是{timeSpan.TotalMinutes}分钟, 请调整系统时间, 否则可能会造成通知不精确.";
                    Messenger.SendDebugInfo(msg);
                    Console.WriteLine(msg);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("时间检查出错:");
                Console.WriteLine(e);
            }

        }

        public bool Inited { get; private set; }
        public static bool UseTestConnector { get; set; }

        private void InitLogger()
        {
            if (!Program.NoLog)
            {
                Directory.CreateDirectory("WFBotLogs");
                var fileListener = new TextWriterTraceListener(File.Open(Path.Combine($"WFBotLogs", $"WFBot-{DateTime.Now:yy-MM-dd_HH.mm.ss}.log"),
                        FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                { TraceOutputOptions = TraceOptions.Timestamp };
                Trace.Listeners.Add(fileListener);
            }
            Trace.Listeners.Add(new ConsoleTraceListener());
            Trace.AutoFlush = true;
        }

        internal List<WFBotTimer> timers = new List<WFBotTimer>();
        private void InitTimer()
        {
            AddTimer<NotificationTimer>();
            AddTimer<WFResourcesTimer>();

            void AddTimer<T>() where T : WFBotTimer
            {
                timers.Add(Activator.CreateInstance<T>());
            }
        }


    }
}

