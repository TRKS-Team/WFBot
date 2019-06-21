using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TRKS.WF.QQBot.MahuaEvents;

namespace TRKS.WF.QQBot
{
    public static class AutoUpdateRR
    {
        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
        private static extern UInt32 DnsFlushResolverCache();


        public static void Execute()
        {
            DnsFlushResolverCache();
            var basePath = "YUELUO\\TRKS.WF.QQBot\\AutoUpdater.exe";
            var path = Path.GetTempFileName() + ".exe";
            File.Copy(basePath, path, true);
            File.Copy(basePath+".config", path + ".config", true);
            Process.Start(new ProcessStartInfo(path)
            {
                WorkingDirectory = Directory.GetCurrentDirectory(),
                UseShellExecute = false
            });

            Trace.WriteLine("Started AutoUpdate.", "AutoUpdate");
        }
    }
}