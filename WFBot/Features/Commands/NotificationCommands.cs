using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFBot.Features.Common;
using WFBot.Features.ImageRendering;
using WFBot.Features.Other;
using WFBot.Features.Utils;
using WFBot.TextCommandCore;
using WFBot.Utils;

namespace WFBot.Features.Commands
{

    public partial class CommandsHandler
    {
        WFNotificationHandler WFNotificationHandler => WFBotCore.Instance.NotificationHandler;

        [Matchers("警报")]
        [AddPlatformInfo]
        async Task Alerts()
        {
            try
            {
                await WFNotificationHandler.UpdateAlertPool();
            }
            catch (OperationCanceledException)
            {
                Append("操作超时.");
                return;
            }
            var alerts = WFNotificationHandler.AlertPool;

            AppendLine("指挥官, 下面是太阳系内所有的警报任务, 供您挑选.");
            foreach (var alert in alerts)
            {
                AppendLine(WFFormatter.ToString(alert));
                AppendLine();
            }

            if (AsyncContext.GetUseImageRendering())
            {
                SendImage(ImageRenderHelper.SimpleImageRendering(OutputStringBuilder.Value.ToString()));
                OutputStringBuilder.Value.Clear();
            }
        }

        [Matchers("入侵")]
        [AddPlatformInfo]
        async Task Invasions()
        {
            if (AsyncContext.GetUseImageRendering())
            {
                var i = ImageRenderingPGO.Invasion();
                if (i != null)
                {
                    SendImage(i);
                    return;
                }
            }
            try
            {
                await WFNotificationHandler.UpdateInvasionPool();
            }
            catch (OperationCanceledException)
            {
                Append("操作超时.");
                return;
            }
            var invasions = WFNotificationHandler.InvasionPool;

            if (!AsyncContext.GetUseImageRendering())
            {
                AppendLine("指挥官, 下面是太阳系内所有的入侵任务.");
                AppendLine();

                foreach (var invasion in invasions.Where(invasion => !invasion.completed))
                {
                    AppendLine(WFFormatter.ToString(invasion));
                    AppendLine();
                }
            }
            else
            {
                SendImage(ImageRenderHelper.Invasion(invasions.Where(invasion => !invasion.completed)));
            }
            
        }

        [Matchers("小小黑", "追随者")]
        async Task AllPersistentEnemies()
        {
            // ask toma 为什么这里不需要 UpdatePersistentEnemiePool
            // 我他妈忘了
            try
            {
                await WFNotificationHandler.UpdatePersistentEnemiePool();
            }
            catch (OperationCanceledException)
            {
                Append("操作超时.");
                return;
            }
            var enemies = WFNotificationHandler.StalkerPool;
            if (!enemies.Any())
            {
                Append("目前没有小小黑出现.");
                return;
            }
            
            AppendLine("下面是全太阳系内的小小黑, 快去锤爆?");
            foreach (var enemy in enemies)
            {
                AppendLine(WFFormatter.ToString(enemy));
            }
            if (AsyncContext.GetUseImageRendering())
            {
                SendImage(ImageRenderHelper.SimpleImageRendering(OutputStringBuilder.Value.ToString()));
                OutputStringBuilder.Value.Clear();
            }
        }

    }
}