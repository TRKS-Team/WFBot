using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
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
            // todo 自动下载miraiconnector
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

            if (connectors.Length > 1) throw new Exception("连接器数目大于1, 你只能同时使用一个连接器.");
            if (connectors.Length == 0) throw new Exception("没有找到连接器, 如果你不知道这是啥, 请看部署文档.");
            Connector = (WFBotConnectorBase)Activator.CreateInstance(connectors.Single());
            Connector.Init();
        }
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
