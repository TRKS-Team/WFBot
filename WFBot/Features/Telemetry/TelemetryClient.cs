using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WFBot.Features.Telemetry
{
    public static class TelemetryClient
    {
        public static void Start()
        {
            if (!Config.Instance.UseTelemetry) return;
            
            Console.WriteLine("此版本 WFBot 带有数据收集来改进用户体验. 收集的内容包括基本系统信息、启动时间、命令处理异常、是否在线等, 不包含各类 QQ 号和消息. " +
                              "如果你不喜欢, 可以在 Config.json 中关闭.");
            Task.Delay(TimeSpan.FromHours(1)).ContinueWith((t) =>
            {
                
            });
            throw new NotImplementedException("还没写完呢");
        }

    }
}
