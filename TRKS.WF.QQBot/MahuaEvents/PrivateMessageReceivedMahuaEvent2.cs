using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newbe.Mahua.MahuaEvents;
using Newbe.Mahua;
using AutoUpdater;
using Newbe.Mahua.Internals;
using Settings;
using MahuaPlatform = Newbe.Mahua.Internals.MahuaPlatform;

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

        private void DownloadImage(string filename, string path, string name)
        {
            if (MahuaPlatformValueProvider.CurrentPlatform.Value == MahuaPlatform.Cqp)
            {
                var file = File.ReadAllLines(Path.Combine(@"data\image", filename));
                foreach (var line in file)
                {
                    if (line.StartsWith("url"))
                    {
                        var url = line.Substring(4);
                        WebHelper.DowloadFile(url, path, name);
                    }
                }
            }
        }
        public void ProcessPrivateMessage(PrivateMessageReceivedContext context)
        {
            if (HotUpdateInfo.PreviousVersion) return;
            // F**k 我先注释了 如果你有兴趣拿正则重构这傻逼功能 JUSTDOIT.
            /*if (context.Message.StartsWith("添加骚话"))
            {
                var strs = context.Message.Split(' ');
                if (strs.Length >= 2)
                {
                    var word = strs[1];
                    var result = strs[2];
                    if (result.Contains("[CQ:image"))// 检测是否有图片cq码
                    {
                        var strs1 = strs[2].Split(new[] {"file="}, StringSplitOptions.None);// 去掉cq码前面的东西
                        var strs2 = strs1.Last().Split('.');// 去掉cq码后面的东西
                        var filename = strs2.First() + ".cqimg";// 终于 我们得到了cq随机生成的文件名 还得加个cqimg 
                        if (!Directory.Exists(@"骚话\"))
                        {
                            Directory.CreateDirectory(@"骚话\");
                        }

                        var extension = strs2.Last().Substring(0, 3);// 文件扩展名
                        DownloadImage(filename, @"骚话\", word + "." + extension);// 下载图片
                        result = strs[2].Replace(strs2.First(), @"骚话\" + word);// 原本的缓存重新导向
                    }
                    CoquettishConfig.Instance.CoquettishWords.Add(new CoquettishWord(word, result));
                    CoquettishConfig.Save();
                }
                else
                {
                    Messenger.SendPrivate(context.FromQq, "参数不足,添加骚话请使用: 添加骚话 关键词 骚话");
                }
            }*/

            if (context.Message.StartsWith("删除骚话"))
            {
                var strs = context.Message.Split(' ');
                if (strs.Length >= 2)
                {
                    CoquettishConfig.Instance.CoquettishWords.Remove(CoquettishConfig.Instance.CoquettishWords
                        .Where(word => word.word == strs[1]).ToList().First());
                    Messenger.SendPrivate(context.FromQq, "已经移除.");
                }
            }
            if (context.FromQq == Config.Instance.QQ)
            {
                if (context.Message == $"没有开启通知的群 {Config.Instance.Code}")
                {
                    var groups = _mahuaApi.GetGroupsWithModel().Model.ToList();
                    var gs = groups.Where(g => !groups.Contains(g)).Select(g => $"{g.Group}-{g.Name}");

                    Messenger.SendPrivate(context.FromQq, string.Join("\r\n", gs));
                }
                if (context.Message == $"执行自动更新 {Config.Instance.Code}")
                {
                    AutoUpdateRR.Execute();
                }
            }

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
                                Messenger.SendPrivate(context.FromQq, "群号已经存在.");
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
                            Messenger.SendPrivate(context.FromQq, "您群号真牛逼."); // 看一次笑一次 3
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
