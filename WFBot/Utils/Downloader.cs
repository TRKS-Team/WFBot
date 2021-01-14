using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

#pragma warning disable 4014

namespace WFBot.Utils
{
    public static class Downloader
    {
        private static volatile int expectedCount = 0;
        private static volatile int finishedCount = 0;

        public static async Task GetCacheOrDownload<T>(string url, Action<T> setter)
        {
            Directory.CreateDirectory("WFCaches");
            var fileName = url.Split('/').Last().Split('?').First().Split("!").First();
            var path = Path.Combine("WFCaches", fileName);
            expectedCount++;

            if (File.Exists(path))
            {
                try
                {
                    setter(File.ReadAllText(path).JsonDeserialize<T>());

                    // 这里下载并刷新缓存
                    Task.Factory.StartNew(async () => await Download(url, setter, path).ConfigureAwait(false), TaskCreationOptions.LongRunning);
                    return;
                }
                catch (Exception)
                {
                    Trace.WriteLine($"缓存喂狗了, 正在重新下载...", "Cache");
                }
            }

            await Download(url, setter, path);

        }
        public static async Task GetCacheOrDownload<T>(string url, Action<T> setter, WebHeaderCollection header)
        {
            Directory.CreateDirectory("WFCaches");
            var fileName = url.Split('/').Last().Split('?').First().Split("!").First();
            var path = Path.Combine("WFCaches", fileName);
            expectedCount++;

            if (File.Exists(path))
            {
                try
                {
                    setter(File.ReadAllText(path).JsonDeserialize<T>());

                    // 这里下载并刷新缓存
                    Task.Factory.StartNew(async () => await Download(url, setter, path, header).ConfigureAwait(false), TaskCreationOptions.LongRunning);
                    return;
                }
                catch (Exception)
                {
                    Trace.WriteLine($"缓存喂狗了, 正在重新下载...", "Cache");
                }
            }

            await Download(url, setter, path, header);

        }
        public static async Task GetCacheOrDownload<T>(string url, Action<T> setter, WebHeaderCollection header, string fileName)
        {
            Directory.CreateDirectory("WFCaches");
            var path = Path.Combine("WFCaches", fileName);
            expectedCount++;

            if (File.Exists(path))
            {
                try
                {
                    setter(File.ReadAllText(path).JsonDeserialize<T>());

                    // 这里下载并刷新缓存
                    Task.Factory.StartNew(async () => await Download(url, setter, path, header).ConfigureAwait(false), TaskCreationOptions.LongRunning);
                    return;
                }
                catch (Exception)
                {
                    Trace.WriteLine($"缓存喂狗了, 正在重新下载...", "Cache");
                }
            }

            await Download(url, setter, path, header);

        }


        private static async Task Download<T>(string url, Action<T> setter, string path)
        {
            T content = default;
            try
            {
                content = await WebHelper.DownloadJsonAsync<T>(url).ConfigureAwait(false);

            }
            catch (Exception e)
            {
                Trace.WriteLine($"词典文件下载失败: {e}");
            }
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
                Trace.WriteLine("可缓存的玩意儿看起来全部下载完成.");
            }
        }
        private static async Task Download<T>(string url, Action<T> setter, string path, WebHeaderCollection header)
        {
            T content = default;
            try
            {
                content = await WebHelper.DownloadJsonAsync<T>(url, header).ConfigureAwait(false);

            }
            catch (Exception e)
            {
                Trace.WriteLine($"词典文件下载失败: {e}");
            }
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
                Trace.WriteLine("可缓存的玩意看起来全部下载完成.");
            }
        }
    }
}
