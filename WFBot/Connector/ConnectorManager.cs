using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

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
                .Select(file => new PluginLoadContext(Path.GetFullPath(file)).LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(file))).ExportedTypes)
                .SelectMany(types=>types)
                .Where(type => type.IsSubclassOf(typeof(WFBotConnectorBase)))
                .ToArray();
            if (connectors.Length != 1) throw new Exception("连接器数目不为1, 你只能同时使用一个连接器.");
            Connector = (WFBotConnectorBase)Activator.CreateInstance(connectors.Single());
            Connector.Init();
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
