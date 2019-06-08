using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Extensions;

namespace TRKS.WF.QQBot
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
    public static class WebHelper
    {
        static WebHelper()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private static ThreadLocal<WebClient> webClient = new ThreadLocal<WebClient>(() =>
        {
            var client = new WebClientEx2();
            client.DownloadStringCompleted += (sender, args) =>
            {
                Trace.WriteLine(
                    $"Download data completed: Size [{Encoding.UTF8.GetByteCount(args.Result) / 1024.0:N1}KB].",
                    "Downloader");
            };
            return client;
        });

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
            return new WebStatus(false, 666);
        }

        public static T DownloadJson<T>(string url, bool throwException = true)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var count = 2;
                while (count-- > 0)
                {
                    try
                    {
                        return new HttpClient().GetStringAsync(url).Result.JsonDeserialize<T>();
                    }
                    catch (Exception)
                    {
                    }
                }

                if (throwException)
                {
                    throw new WebException($"在下载[{url}]时多次遇到问题. 请检查你的网络是否正常或联系项目负责人.");
                }
                else
                {
                    return default;
                }
            }
            finally
            {
                Trace.WriteLine($"Download data completed: URL [{url}], Time [{sw.ElapsedMilliseconds}ms].", "Downloader");
            }
        }

        public static T DownloadJsonParallel<T>(params string[] urls)
        {
            var tasks = urls.Select(url => Task.Run(() => DownloadJson<T>(url, false))).ToList();

            while (tasks.Any())
            {
                Task.WaitAny(tasks.ToArray<Task>());
                foreach (var task in tasks.Where(task => task.IsCompleted && task.Result != null))
                    return task.Result;

                tasks.RemoveAll(task => task.IsCompleted && task.Result == null);
            }

            throw new WebException($"在下载[{urls.FirstOrDefault()}]时多次遇到问题. 请检查你的网络是否正常或联系项目负责人.");
        }

        public static async Task<T> DownloadJsonAsync<T>(string url)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var count = 3;
                while (count-- > 0)
                {
                    try
                    {
                        return (await webClient.Value.DownloadStringTaskAsync(url)).JsonDeserialize<T>();
                    }
                    catch (Exception)
                    {
                    }
                }
                throw new WebException($"在下载[{url}]时多次遇到问题. 请检查你的网络是否正常或联系项目负责人.");
            }
            finally
            {
                Trace.WriteLine($"Download data completed: URL [{url}], Time [{sw.ElapsedMilliseconds}ms].", "Downloader");
            }
        }

        public static T DownloadJson<T>(string url, WebHeaderCollection header)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var wc = new WebClientEx2();
                wc.Headers = header;
                return wc.DownloadString(url).JsonDeserialize<T>();
            }
            finally
            {
                Trace.WriteLine($"Download data completed: URL [{url}], Time [{sw.ElapsedMilliseconds}ms].", "Downloader");
            }
        }

        public static T UploadJson<T>(string url, string body)
        {
            return webClient.Value.UploadString(url, body).JsonDeserialize<T>();
        }

        public static T UploadJson<T>(string url, string body, WebHeaderCollection header)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var wc = new WebClientEx2 { Headers = header };
                return wc.UploadString(url, body).JsonDeserialize<T>();
            }
            finally
            {
                Trace.WriteLine($"Download data completed: URL [{url}], Time [{sw.ElapsedMilliseconds}ms].", "Downloader");
            }
        }

        public static void DownloadFile(string url, string path, string name)
        {
            Directory.CreateDirectory(path);
            webClient.Value.DownloadFile(url, Path.Combine(path, name));
            /*var img = Image.FromFile(Path.Combine(path, name));
            var fullname = name;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
            {
                fullname = name + ".gif";
            }
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
            {
                fullname = name + ".jpg";
            }
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
            {
                fullname = name + ".png";
            }
            webClient.Value.DownloadFile(url, Path.Combine(path, fullname));*/
        }
    }

    public class WebClientEx2 : WebClient
    {
        public WebClientEx2()
        {
            Encoding = Encoding.UTF8;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var rq = (HttpWebRequest)base.GetWebRequest(address);
            if (rq != null)
            {
                rq.KeepAlive = false;
            }
            return rq;
        }
    }
}
