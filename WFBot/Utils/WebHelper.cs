using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Enhancements;
using GammaLibrary.Extensions;
using Newtonsoft.Json;
using TextCommandCore;
using WFBot.Features.Resource;

namespace WFBot.Utils
{
    public class WebStatus
    {
        public WebStatus(bool isOnline, long latency)
        {
            IsOnline = isOnline;
            Latency = latency;
        }

        public Boolean IsOnline { get; set; }
        public long Latency { get; set; }
    }

    public class Container<T> where T : struct
    {
        public Container(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }

    public static class WebHelper
    {
        public static WebStatus TryGet(string url)
        {
            var count = 3;
            while (count-- > 0)
            {
                try
                {
                    var client = new HttpClient();
                    var sw = Stopwatch.StartNew();
                    var response = client.GetAsync(url).Result;
                    return new WebStatus(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Unauthorized, sw.ElapsedMilliseconds);
                }
                catch (Exception)
                {
                    //
                }
            }
            return new WebStatus(false, 6666666666/*???*/);
        }
        
        
        public static async Task<T> DownloadJsonAsync<T>(string url, WebHeaderCollection header = null, TimeSpan? timeout = null)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var hc = new HttpClient(new RetryHandler(new HttpClientHandler()));
                header ??= new WebHeaderCollection();
                foreach (string key in header)
                {
                    hc.DefaultRequestHeaders.Add(key, header[key]);
                }
                hc.Timeout = timeout ?? TimeSpan.FromSeconds(25);

                try
                {
                    var data = await hc.GetAsync(url, AsyncContext.GetCancellationToken());
                    data.EnsureSuccessStatusCode();

                    return await ResourceLoaders<T>.JsonDotNetLoader(await data.Content.ReadAsStreamAsync());
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.Timeout)
                    {
                        throw new CommandException($"请求超时.");
                    }

                    throw new CommandException($"数据下载出错: {e.Message}.");
                }
            }
            finally
            {
                Trace.WriteLine($"数据下载完成: URL '{url}', 用时 '{sw.Elapsed.TotalSeconds:F1}s'.", "Downloader");
            }
        }
    }
}
