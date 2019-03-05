using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable 4014

namespace TRKS.WF.QQBot
{
    public static class Downloader
    {
        private static volatile int expectedCount = 0;
        private static volatile int finishedCount = 0;

        public static Task GetCacheOrDownload<T>(string url, Action<T> setter)
        {
            return Task.Run(() =>
            {
                Directory.CreateDirectory("WFCaches");
                var fileName = url.Split('/').Last();
                var path = $"WFCaches\\{fileName}";
                expectedCount++;

                if (File.Exists(path))
                {
                    try
                    {
                        setter(File.ReadAllText(path).JsonDeserialize<T>());

                        // 这里下载并刷新缓存
                        Task.Run(() => Download(url, setter, path));
                        return;
                    }
                    catch (Exception)
                    {
                        Trace.WriteLine($"缓存喂狗了, 正在重新下载...", "Cache");
                    }
                }

                Download(url, setter, path);
            });
        }

        private static void Download<T>(string url, Action<T> setter, string path)
        {
            var content = WebHelper.DownloadJson<T>(url);
            try
            {
                File.WriteAllText(path, content.ToJsonString());
            }
            catch (Exception e)
            {
                Trace.WriteLine($"我操我上辈子是日了狗么, 缓存 {path} 无法保存. {e}", "Cache"); //哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈啊哈哈哈啊哈哈哈哈哈哈哈哈哈哈 --2019年3月2日22:18:27
            }
            setter(content);
            finishedCount++;

            if (finishedCount == expectedCount && expectedCount >= 6)
            {
                Messenger.SendDebugInfo("可缓存的玩意看起来全部下载完成.");
            }
        }
    }
}
