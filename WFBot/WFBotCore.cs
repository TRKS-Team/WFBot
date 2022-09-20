using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using GammaLibrary.Extensions;
using InternetTime;
using WFBot.Events;
using WFBot.Features.CustomCommandContent;
using WFBot.Features.Events;
using WFBot.Features.Other;
using WFBot.Features.Resource;
using WFBot.Features.Telemetry;
using WFBot.Features.Timers;
using WFBot.Features.Timers.Base;
using WFBot.Features.Utils;
using WFBot.Orichalt;
using WFBot.TextCommandCore;
using WFBot.Utils;
using WFBot.WebUI;
using WFBot.Windows;
using static System.Net.Mime.MediaTypeNames;

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
#if DEBUG
            setCurrentFolder = true;
#endif
            if (setCurrentFolder)
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            }


            //return;

            var wfbot = new WFBotCore();
            try
            {
                await wfbot.Init();
            }
            catch (Exception e)
            {
                Trace.WriteLine("WFBot 在初始化中遇到了问题.");
                Trace.WriteLine($"你现在可以在 WebUI http://localhost:{WFBotWebUIServer.GetServerPort()} 中修改设置.");
                Trace.WriteLine(e);
                Panic = true;
                var sw = Stopwatch.StartNew();
                if (!skipPressKey)
                {
                    Console.WriteLine("按任意键继续.");
                    Console.ReadKey();
                }

                if (sw.ElapsedMilliseconds < 200)
                {
                    Console.WriteLine("触发 Console.ReadKey 的时间较短, 可能是在 docker 下运行, 为了保证 WebUI 的正常运行, 将不结束程序.");
                    Thread.CurrentThread.Join();
                }
                return;
            }

            wfbot.Run();
        }

        public static bool Panic { get; private set; } = false;
    }
    public sealed class WFBotCore
    {
        public WFBotCore()
        {
            Instance = this;
        }
        public WFBotCore(bool istest)
        {
            Instance = this;
            IsTest = istest;
        }
        public WFNotificationHandler NotificationHandler { get; private set; }
        public static WFBotCore Instance { get; internal set; }
        public MessageReceivedEvent messageReceivedEvent;
        public PrivateMessageReceivedEvent privateMessageReceivedEvent;
        public static string Version { get; }
        public static bool IsOfficial { get; }
        public static bool IsShuttingDown { get; private set; }
        public static bool IsTest { get; private set; }
        public static WFBotWebUIServer WebUIServer { get; private set; }
        public static DateTime StartTime { get; } = DateTime.Now;

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

        public void Run()
        {
            while (true)
            {
                var text = Console.ReadLine();
                if (text == null)
                {
                    Trace.WriteLine("WORKAROUND: 检测到空控制台输入，将不再读取控制台输入。这可能是因为 WFBot 运行在 Docker 下。如果你在正常情况下看到这个错误，请汇报它。");
                    Thread.CurrentThread.Join();
                }

                OnConsoleCommand(text);
            }
        }

        public void OnConsoleCommand(string s)
        {
            switch (s.ToLower())
            {
                case "ui":
                    OpenWFBotSettingsWindow();
                    break;
                case "webui":
                    OpenWebUI();
                    break;
                case "exit":
                case "stop":
                    Shutdown();
                    return;
                default:
                    if (!(new CustomCommandMatcherHandler(s.TrimStart('/'))).ProcessCommandInput().Result.matched)
                    {
                        // todo ConnectorManager.Connector.OnCommandLineInput(text);
                    }
                    break;
            }
        }

        void OpenWebUI()
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo($"http://localhost:{WFBotWebUIServer.GetServerPort()}")
                    { UseShellExecute = true });
            }
            else
            {
                Console.WriteLine("你使用的不是 Windows, 请手动打开 " + $"http://localhost:{WFBotWebUIServer.GetServerPort()}");
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

        public async Task OnGroupMessage(OrichaltContext o)
        {
            if (!Inited)
            {
                if (Program.Panic)
                {
                    Trace.WriteLine($"接收到消息{o.GetInfo()}, 但是WFBot在初始化中遇到了问题, 请向上翻.");   
                }
                else
                {
                    Trace.WriteLine($"由于 WFBot 未初始化完, 接收到的消息无法处理: {o.GetInfo()}");
                }
                // 为啥这句要写英文?
                return;
            }
            await messageReceivedEvent.ProcessGroupMessage(o);
        }

        public void OnFriendMessage(OrichaltContext o)
        {
            if (!Inited) return;
            privateMessageReceivedEvent.ProcessPrivateMessage(o);
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
            Config.Save();

            _ = TelemetryClient.Start();
            // 初始化 WebUI
            WebUIServer = new WFBotWebUIServer();
            WebUIServer.Run();

            if (Config.Instance.Miguel_Platform == MessagePlatform.Unknown && !IsTest)
            {
                Trace.WriteLine("看起来你是第一次使用WFBot, 请在WFConfig.json里修改\"Miguel_Platform\"项, 聊天平台对应关系: 0.OneBot 1.Kaiheila 2.QQ频道 3.MiraiHTTPv2");
                Trace.WriteLine("你也可以使用 WebUI 来进行设置，详情请查看文档.");
                Trace.WriteLine("设置完后请重启 WFBot.");
                if (OperatingSystem.IsWindows())
                {
                    Process.Start(new ProcessStartInfo($"http://localhost:{WFBotWebUIServer.GetServerPort()}")
                        {UseShellExecute = true});
                }
                Thread.CurrentThread.Join();
            }
            /*while (Config.Instance.Miguel_Platform == MessagePlatform.Unknown && !IsTest)
            {
                Console.WriteLine("看起来你是第一次使用WFBot, 请通过数字序号指定聊天平台, 0.OneBot(Mirai) 1.Kaiheila 2.QQ频道 3.MiraiHTTPv2");
                /*var platformstr = Console.ReadLine();
                if (platformstr.IsNumber() && platformstr.ToInt() <= 3 && 0 <= platformstr.ToInt())
                {
                    Config.Instance.Miguel_Platform = (MessagePlatform)platformstr.ToInt();
                    Config.Save();
                }
            }*/
            switch (Config.Instance.Miguel_Platform)
            {
                case MessagePlatform.OneBot:
                    Trace.WriteLine("服务协议: Onebot");
                    break;
                case MessagePlatform.MiraiHTTP:
                    Trace.WriteLine("服务协议: MiraiHTTPv2");
                    break;
                case MessagePlatform.MiraiHTTPV1:
                    Trace.WriteLine("服务协议: MiraiHTTPv1");
                    break;
                case MessagePlatform.Kaiheila:
                    Trace.WriteLine("服务协议: 开黑啦");
                    break;
                case MessagePlatform.QQChannel:
                    Trace.WriteLine("服务协议: QQ频道");
                    break;
                case MessagePlatform.Test:
                    Trace.WriteLine("服务协议: 测试模式");
                    break;
            }
            Trace.WriteLine("加载米格尔网络...");
            _ = Task.Run(() =>
                MiguelNetwork.InitMiguelNetwork(IsTest ? MessagePlatform.Test : Config.Instance.Miguel_Platform));

            Trace.WriteLine("加载资源...");
            await WFResources.InitWFResource();
            messageReceivedEvent = new MessageReceivedEvent();
            privateMessageReceivedEvent = new PrivateMessageReceivedEvent();
            NotificationHandler = new WFNotificationHandler();

            // 加载自定义命令处理器
            Trace.WriteLine("加载自定义命令处理器...");
            CustomCommandMatcherHandler.InitCustomCommandHandler();

            Trace.WriteLine("加载自定义命令内容处理器...");
            try
            {
                CustomCommandContentHandler.Load();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            // 检查时间...
            _ = Task.Run(() => CheckTime());

            // 初始化定时器
            Trace.WriteLine("初始化定时器...");
            InitTimer();

            // ------------------------------------------------------------------
            // 完成
            Inited = true;
            if (_requestedCtrlCShutdown)
            {
                _ = Task.Run(() => Shutdown());
            }

            _requestedCtrlCShutdown = false;
            Trace.WriteLine("");
            var startTime = sw.Elapsed.TotalSeconds;
            StartUpTime = startTime.ToString("F1")+"s";
            string t = "";
            try
            {
                var hc = new HttpClient();
                var s = await hc.GetStringAsync($"https://wfbot.cyan.cafe/api/StartUpTime?time={startTime:F4}");
                t = s;
            }
            catch (Exception)
            {
            }
            Messenger.SendDebugInfo($"<<<<   WFBot 加载完成. 用时 {startTime:F1}s. {t}  >>>>");
            Trace.WriteLine("WebUI 在 "+ $"http://localhost:{WFBotWebUIServer.GetServerPort()}" +" 启用.");
            Trace.WriteLine("");
            if (TelemetryClient.connected)
            {
                TelemetryClient.ReportStarted(startTime);
            }
        }

        public static string StartUpTime { get; private set; }
        public static TimeSpan TimeDelayFromRealTime { get; private set; }
        public volatile static int InstanceMessagesProcessed = 0;
        public volatile static int InstanceCommandsProcessed = 0;
        void CheckTime()
        {
            try
            {
                if (TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Hours != 8)
                {
                    var msg = "**************警告: 你的系统时区不为 UTC+8, 会造成任务通知不精确.**************";
                    Messenger.SendDebugInfo(msg);
                }
                var sntpClient = new SNTPClient("ntp.aliyun.com");
                sntpClient.Connect(false);
                var timeSpan = TimeSpan.FromMilliseconds(sntpClient.LocalClockOffset) + TimeSpan.FromHours(8);
                TimeDelayFromRealTime = timeSpan;
                if (timeSpan.TotalMinutes is > 1 or < -1)
                {
                    var msg = $"*************警告: 你的系统时间与世界时间相差了1分钟以上, 具体来说是{timeSpan.TotalMinutes}分钟, 请调整系统时间, 否则可能会造成通知不精确.**************";
                    Messenger.SendDebugInfo(msg);
                    Trace.WriteLine(msg);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("时间检查出错:");
                Trace.WriteLine(e);
            }

        }

        public bool Inited { get; private set; }

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
            Trace.Listeners.Add(new WebLogTraceListener());
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

