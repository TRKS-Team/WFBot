using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using HarmonyLib;
using WFBot.MahuaEvents;

namespace WFBot.Utils
{
    public class Plugins
    {
        public static void Load()
        {
            var harmony = new Harmony($"wfbot");
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
