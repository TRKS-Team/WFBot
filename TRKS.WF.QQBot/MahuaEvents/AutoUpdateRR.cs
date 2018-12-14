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
            Process.Start(new ProcessStartInfo("YUELUO\\TRKS.WF.QQBot\\AutoUpdater.exe")
            {
                WorkingDirectory = Directory.GetCurrentDirectory()
            });
            
            GroupMessageReceivedMahuaEvent1._wFAlert?.timer?.Stop();
            InitEvent1.timer1?.Stop();
        }
    }
}
