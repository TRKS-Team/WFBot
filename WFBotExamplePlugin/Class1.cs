using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using TRKS.WF.QQBot;
using TRKS.WF.QQBot.MahuaEvents;

namespace WFBotExamplePlugin
{
    // 请注意! 由于技术原因, MahuaEvents 文件夹里面的以文件名命名的类无法被修改.
    // 如 GroupMessageHandler 可以被修改
    // 而 GroupMessageReceivedMahuaEvent1 无法被修改

    // 关于改方法的更多信息请查看 Harmony 库的 [这里](https://github.com/pardeike/Harmony/wiki/Patching)

    [HarmonyPatch(typeof(GroupMessageHandler))]        // 这里写你想修改的方法所在的类
    [HarmonyPatch("FortunaMissions")]        // 这里写你想修改方法的名字
    [HarmonyPatch(new[] { typeof(int) })] // 这里写这个方法的参数列表
    class Patch
    {
        // Prefix 的名字 不应修改
        static bool Prefix(int index = 0)  // (非特殊)参数排序及参数名必须与原方法相同 返回值为 bool 的意思是 如果返回 false 就不会再执行原方法
        {
            Console.WriteLine("金星赏金 is fucked");
            return false;
        }
    }

    // 这里的演示会让翻译器给出的结果全部为 AWSL
    [HarmonyPatch(typeof(Translator))]
    [HarmonyPatch("Translate")]
    [HarmonyPatch(new[] { typeof(string) })]
    class Patch2
    {
        // __instance 为该方法执行时的 this
        // __result 是如果你想修改这个方法的返回值 改这个
        // 更多信息请查看 https://github.com/pardeike/Harmony/wiki/Patching
        static bool Prefix(Translator __instance, ref string __result, string source)
        {
            Console.WriteLine("Translator is fucked");
            __result = $"AWSL: {source}";
            return false;
        }
    }

    [HarmonyPatch(typeof(WFFormatter))]
    [HarmonyPatch("Translate")]
    [HarmonyPatch(new[] { typeof(string) })]
    class Patch3
    {
        // __instance 为该方法执行时的 this
        // __result 是如果你想修改这个方法的返回值 改这个
        // 更多信息请查看 https://github.com/pardeike/Harmony/wiki/Patching
        static bool Prefix(Translator __instance, ref string __result, string source)
        {
            Console.WriteLine("Translator is fucked");
            __result = $"AWSL: {source}";
            return false;
        }
    }
}
