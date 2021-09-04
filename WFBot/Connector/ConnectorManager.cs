using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Mirai_CSharp;
using Mirai_CSharp.Extensions;
using Mirai_CSharp.Models;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Connector
{
    public static class ConnectorManager
    {
        public static WFBotConnectorBase Connector { get; private set; }
        public static void LoadConnector()
        {
#pragma warning disable CS0618 
            AppDomain.CurrentDomain.AppendPrivatePath("WFBotConnector");

            Directory.CreateDirectory("WFBotConnector");

            var connectors = Directory.GetFiles("WFBotConnector", "*.dll")
                .Select(file => //Program.Context == null 
                    /*?*/ new PluginLoadContext(Path.GetFullPath(file)).LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(file))).ExportedTypes
                    //: ((Assembly)Program.Context.Load(file)).ExportedTypes
                    )
                .SelectMany(types=>types)
                .Where(type => type.IsSubclassOf(typeof(WFBotConnectorBase)))
                .ToArray();
            //Trace.WriteLine($"连接器个数有 {connectors.Length} 个.");
            if (WFBotCore.UseTestConnector) connectors = new Type[] {typeof(TestConnector)};
            if (connectors.Length == 0)
            {
                Console.WriteLine("没有找到连接器, 将默认使用 Mirai 连接器.");
                connectors = new Type[] { typeof(MiraiConnector) };
            }
            if (connectors.Length > 1) throw new Exception("连接器数目大于1, 你只能同时使用一个连接器.");
            //if (connectors.Length == 0) throw new Exception("没有找到连接器, 如果你不知道这是啥, 请看部署文档.");
            Connector = (WFBotConnectorBase)Activator.CreateInstance(connectors.Single());
            Connector.Init();
        }
    }

    public class MiraiConnector : WFBotConnectorBase
    {
        private MiraiHttpSession session;

        public override void Init()
        {
            var config = MiraiConfigInMain.Instance;
            var qq = config.BotQQ;
            var host = config.Host;
            var port = config.Port;
            var authKey = config.AuthKey;
            if (Directory.Exists("WFBotImageCaches"))
            {
                Directory.Delete("WFBotImageCaches", true);
            }

            if (qq == default || host == default || port == default || authKey == default)
            {
                // todo 直接控制台写
                throw new InvalidOperationException("请在 MiraiConfig.json 内补全信息, 详情请查看文档.");
            }

            var options = new MiraiHttpSessionOptions(host, port, authKey); // 至少八位数
            session = new MiraiHttpSession();
            session.GroupMessageEvt += (sender, args) =>
            {
                var msg = args.Chain.GetPlain();
                ReportGroupMessage(args.Sender.Group.Id, args.Sender.Id, msg);
                return Task.FromResult(true);
            };

            session.FriendMessageEvt += (sender, args) =>
            {
                var msg = args.Chain.GetPlain();
                ReportFriendMessage(args.Sender.Id, msg);
                return Task.FromResult(true);
            };

            session.DisconnectedEvt += async (sender, exception) =>
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Mirai 连接断开, 正在重连...");
                        await session.ConnectAsync(options, qq);
                        return true;
                    }
                    catch (Exception)
                    {
                        await Task.Delay(1000);
                    }
                }
            };
            session.ConnectAsync(options, qq).Wait();
        }

        public override void SendGroupMessage(GroupID groupID, string message)
        {
            var msgID = session.SendGroupMessageAsync(groupID, new PlainMessage(message)).Result;
            if (MiraiConfigInMain.Instance.AutoRevoke)
            {
                Task.Delay(MiraiConfigInMain.Instance.RevokeTimeInSeconds)
                    .ContinueWith((t) => { session.RevokeMessageAsync(msgID); });
            }
        }



        public override void SendPrivateMessage(UserID userID, string message)
        {
            session.SendFriendMessageAsync(userID, new PlainMessage(message)).Wait();
        }
    }

    [Configuration("MiraiConfig")]
    public class MiraiConfigInMain : Configuration<MiraiConfigInMain>
    {
        public string Host = "127.0.0.1";
        public ushort Port = 8080;
        public string AuthKey = "";
        public bool AutoRevoke = false;
        public int RevokeTimeInSeconds = 60;
        public long BotQQ = default;
    }

    public class TestConnector : WFBotConnectorBase
    {
        public override void Init()
        {
        }

        public override void SendGroupMessage(GroupID groupID, string message)
        {
            Console.WriteLine($"{message}");
        }

        public override void SendPrivateMessage(UserID userID, string message)
        {
            Console.WriteLine($"{message}");
        }

        public override void OnCommandLineInput(string content)
        {
            ReportGroupMessage("0", "0", content);
            // 宝贝 这里填fork会报错的
        }
    }

    class PluginLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}
