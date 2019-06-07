using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using TRKS.WF.QQBot.MahuaEvents;

namespace TRKS.WF.QQBot
{
    public class Plugins
    {
        public static void Load()
        {
            var harmony = HarmonyInstance.Create($"wfbot{InitEvent1.localVersion}");
            
            Directory.CreateDirectory("WFBotPlugins");
            foreach (var file in Directory.GetFiles("WFBotPlugins", "*.dll"))
            {
                try
                {
                    harmony.PatchAll(Assembly.LoadFile(Path.GetFullPath(file)));
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
            }
        }
    }
}
