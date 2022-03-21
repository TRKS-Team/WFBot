using System;
using System.Collections.Generic;
using System.Management;
using System.Text;

namespace WFBot.Features.Telemetry
{
    public static class HardwareID
    {
        public static string GetCPUID()
        {
            var mbs = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
            var mbsList = mbs.Get();
            var id = "";
            foreach (var o in mbsList)
            {
                var mo = (ManagementObject)o;
                id = mo["ProcessorId"].ToString();
                break;
            }

            return id;
            // 可能为空
        }

        public static string GetDiskID()
        { 
            var dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""c:""");
            dsk.Get();
            var id = dsk["VolumeSerialNumber"].ToString();
            return id;
        }
        // MotherBoard
        public static string GetMBID()
        {
            var mos = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            var moc = mos.Get();
            var serial = "";
            foreach (var o in moc)
            {
                var mo = (ManagementObject)o;
                serial = (string)mo["SerialNumber"];
            }

            return serial;
        }

        public static string GetHWID()
        {
            return GetCPUID() + GetDiskID() + GetMBID();
        }
    }
}
