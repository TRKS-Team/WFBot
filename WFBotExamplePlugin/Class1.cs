using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using TRKS.WF.QQBot.MahuaEvents;

namespace WFBotExamplePlugin
{
    [HarmonyPatch(typeof(GroupMessageHandler))]
    [HarmonyPatch("FortunaMissions")]
    [HarmonyPatch(new Type[] { typeof(int) })]
    class Patch
    {
        static bool Prefix(int index = 0)
        {
            Console.WriteLine("金星赏金 is fucked");
            return false;
        }
    }
}
