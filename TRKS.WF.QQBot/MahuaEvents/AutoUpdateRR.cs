using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRKS.WF.QQBot.MahuaEvents;

namespace TRKS.WF.QQBot
{
    public static class AutoUpdateRR
    {
        public static void Execute()
        {
            var basePath = "YUELUO\\TRKS.WF.QQBot\\AutoUpdater.exe";
            var path = Path.Combine(Path.GetTempPath(), Path.GetFileName(basePath));
            File.Copy(basePath, path, true);
            Process.Start(new ProcessStartInfo(path)
            {
                WorkingDirectory = Directory.GetCurrentDirectory()
            });

            GroupMessageReceivedMahuaEvent1._wFAlert?.Timer?.Stop();
            InitEvent1.timer1?.Stop();
        }
    }
}