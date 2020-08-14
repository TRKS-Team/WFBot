using System;
using System.Linq;
using System.Management;
using GammaLibrary.Extensions;

namespace WFBot.Utils
{
    public class UsageReports
    {


        public static string GetIdentifyID()
        {
            var cpuInfo = string.Empty;
            var mc = new ManagementClass("win32_processor");
            var moc = mc.GetInstances();

            foreach (var mo in moc.OfType<ManagementObject>())
            {
                if (cpuInfo == "")
                {
                    cpuInfo = mo.Properties["processorID"].Value.ToString();
                    break;
                }
            }
            return (cpuInfo + Environment.CurrentDirectory).MD5().ToHexString();
        }
    }
}
