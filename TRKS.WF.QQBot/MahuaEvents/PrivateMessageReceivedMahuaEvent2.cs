using Newbe.Mahua.MahuaEvents;
using Newbe.Mahua;
using Settings;

namespace TRKS.WF.QQBot.MahuaEvents
{
    /// <summary>
    /// 私聊消息接收事件
    /// </summary>
    public class PrivateMessageReceivedMahuaEvent2
        : IPrivateMessageReceivedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;
        
        public PrivateMessageReceivedMahuaEvent2(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessPrivateMessage(PrivateMessageReceivedContext context)
        {
            if (context.Message.Contains("添加群"))
            {
                var strs = context.Message.Split(' ');
                if (strs.Length >= 3)
                {
                    if (strs[1] == Config.Instance.Code)
                    {
                        if (strs[2].IsNumber())
                        {
                            if (Config.Instance.WFGroupList.Contains(strs[2]))
                            {
                                Messenger.SendGroup(context.FromQq, "群号已经存在.");
                            }
                            else
                            {
                                Config.Instance.WFGroupList.Add(strs[2]);
                                Config.Save();
                                Messenger.SendPrivate(context.FromQq, "完事.");
                                Messenger.SendGroup(strs[2], $"{context.FromQq}已经在私聊启用了此群的新任务通知功能.");
                                Messenger.SendDebugInfo($"{strs[2]}启用了通知功能.");
                            }

                        }
                        else
                        {
                            Messenger.SendPrivate(context.FromQq, "您群号真牛逼.");
                        }
                    }
                    else
                    {
                        Messenger.SendPrivate(context.FromQq, "口令错误.");
                    }
                    
                }
                else
                {
                    Messenger.SendPrivate(context.FromQq, "参数不足.");
                }
            }
            if (context.Message.Contains("删除群"))
            {
                var strs = context.Message.Split(' ');
                if (strs.Length >= 3)
                {
                    if (strs[1] == Config.Instance.Code)
                    {
                        if (strs[2].IsNumber())
                        {
                            Config.Instance.WFGroupList.Remove(strs[2]);
                            Config.Save();
                            Messenger.SendPrivate(context.FromQq, "完事.");
                            Messenger.SendGroup(strs[2], $"{context.FromQq}已经在私聊禁用了此群的新任务通知功能.");
                            Messenger.SendDebugInfo($"{strs[2]}禁用了通知功能.");

                        }
                        else
                        {
                            Messenger.SendPrivate(context.FromQq, "您群号真牛逼.");
                        }

                    }
                    else
                    {
                        Messenger.SendPrivate(context.FromQq, "口令错误.");
                    }
                }
                else
                {
                    Messenger.SendPrivate(context.FromQq, "参数不足.");
                }
            }
        }
    }
}
