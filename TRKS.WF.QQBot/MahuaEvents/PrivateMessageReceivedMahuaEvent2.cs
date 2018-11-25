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
                            Config.Instance.WFGroupList.Add(strs[2]);
                            Config.Save();
                            Messenger.SendPrivate(context.FromQq, "Done.");
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
                            Messenger.SendPrivate(context.FromQq, "Done.");
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
