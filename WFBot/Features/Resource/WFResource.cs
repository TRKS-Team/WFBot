﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Enhancements;
using Humanizer;
using Newtonsoft.Json;
using WFBot.Utils;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WFBot.Features.Resource
{
    public static class ResourceLoaders<T>
    {
        public static WFResourceLoader<T> JsonDotNetLoader = stream =>
        {
            // 是不是同步执行然后直接 Task.FromResult好一些?
            return Task.Run(() =>
            {
#if DEBUG
                using var sr = new StreamReader(stream);
                var str = sr.ReadToEnd();
                //修复 http://n9e5v4d8.ssl.hwcdn.net/repos/weeklyRivensPC.json 因PHP警告导致多输出一句话的问题
                //issue #91 https://github.com/TRKS-Team/WFBot/issues/91
                str = str[str.IndexOfAny(new[] { '{', '[' })..];

                return JsonConvert.DeserializeObject<T>(str, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

#else
                using var sr = new StreamReader(stream);
                char c;
                while (!((c = (char) sr.Peek()) == '[' || c == '{')) sr.Read();
                using var reader = new JsonTextReader(sr);
                return new Newtonsoft.Json.JsonSerializer {NullValueHandling = NullValueHandling.Ignore}
                    .Deserialize<T>(reader);
#endif
            });
        };

        public static WFResourceLoader<T> SystemTextJsonLoader = stream => JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: AsyncContext.GetCancellationToken()).AsTask();
    }

    public delegate Task<T> WFResourceLoader<T>(Stream data);
    public delegate Task<Stream> WFResourceRequester(string url);

    public static class WFResourceStatic
    {
        public static readonly ConcurrentDictionary<string, int> CategoryVersionDictionary =
            new ConcurrentDictionary<string, int>();
        internal static int ResourceCount;
        internal static int FinishedResourceCount;
    }

    public class WFResource<T> where T : class
    {
        volatile T value;
        readonly object locker = new object();
        readonly string url;
        readonly WFResourceLoader<T> resourceLoader;
        string CachePath => Path.Combine(CacheDir, FileName);
        string LocalPath => Path.Combine(OfflineDir, FileName);
        readonly WebHeaderCollection header;
        public const string OfflineDir = "WFOfflineResource";
        public const string CacheDir = "WFCaches";

        static WFResource()
        {
            Directory.CreateDirectory(OfflineDir);
            Directory.CreateDirectory(CacheDir);
        }

        protected WFResource(string url = null, string category = null, string fileName = null,
            WebHeaderCollection header = null, WFResourceLoader<T> resourceLoader = null,
            WFResourceRequester wfResourceRequester = null)
        {
            resourceLoader ??= ResourceLoaders<T>.JsonDotNetLoader;
            // 这写的太屎了
            // 屎虽臭但能饱腹
            fileName ??= (url ?? string.Empty).Split('/').Last().Split('?').First().Split("!").First();

            this.resourceLoader = resourceLoader;
            this.url = url;
            FileName = fileName;
            this.header = header;
            Category = category;
            requester = wfResourceRequester ?? RequestResourceFromTheWideWorldOfWeb;
            if (category != null && !WFResourceStatic.CategoryVersionDictionary.ContainsKey(category))
            {
                WFResourceStatic.CategoryVersionDictionary[category] = 0;
            }
            Version = 0;
        }

        public static WFResource<T> Create(string url = null, string category = null, string fileName = null, WebHeaderCollection header = null, WFResourceLoader<T> resourceLoader = null, WFResourceRequester requester = null)
        {
            var result = new WFResource<T>(url, category, fileName, header, resourceLoader, requester);
            Interlocked.Increment(ref WFResourceStatic.ResourceCount);
            result.initTask = result.Reload(true);
            return result;
        }

        Task initTask;
        public Task WaitForInited()
        {
            return initTask ?? Task.CompletedTask;
        }

        public T Value
        {
            get
            {
                lock (locker)
                {
                    return value;
                }
            }

            private set
            {
                lock (locker)
                {
                    this.value = value;
                    Version++;
                    if (Category != null && WFResourceStatic.CategoryVersionDictionary.ContainsKey(Category))
                    {
                        WFResourceStatic.CategoryVersionDictionary[Category]++;
                    }
                }
            }
        }

        public string Category { get; }
        public string FileName { get; }
        public int Version { get; private set; }

        readonly SemaphoreSlim _locker = new SemaphoreSlim(1);
        WFResourceRequester requester;

        public async Task Reload(bool isFirstTime = false)
        {
            using var resourceLock = WFBotResourceLock.Create($"资源刷新 {FileName}");
            try
            {
                await _locker.WaitAsync();
                if (await LoadFromLocal())
                {
                    if (isFirstTime) Trace.WriteLine($"WFResource: 资源 {FileName} 从本地载入.");
                    return;
                }

                if (isFirstTime && await LoadCache())
                {
                    Trace.WriteLine($"WFResource: 资源 {FileName} 从缓存载入.");
                    LoadFromTheWideWorldOfWebNonBlocking();
                    return;
                }

                await LoadFromTheWideWorldOfWeb();
            }
            catch (Exception) when (isFirstTime)
            {
                WFResources.ResourceLoadFailed = true;
                throw;
            }
            finally
            {
                _locker.Release();
                if (isFirstTime)
                {
                    initTask = null;
                }
                Interlocked.Increment(ref WFResourceStatic.FinishedResourceCount);
                Console.WriteLine($">>>> 资源加载进程: {WFResourceStatic.FinishedResourceCount} / {WFResourceStatic.ResourceCount} <<<<");
            }
        }

        async Task LoadFromTheWideWorldOfWeb()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await using var stream = await requester(url);
                var fileStream = File.Open(CachePath + ".tmp", FileMode.Create, FileAccess.Write, FileShare.Read);
                
                try
                {
                    // 尝试将资源保存到缓存再读取...
                    var copyTask = stream.CopyToAsync(fileStream);
                    while (true)
                    {
                        var waitTask = Task.Delay(3.Seconds());
                        await Task.WhenAny(copyTask, waitTask);
                        if (copyTask.IsCompleted) break;
                        Console.WriteLine($"资源 {FileName} 还没有下载完. 下载了 {fileStream.Length.Bytes().Megabytes:F2} MB.");
                    }
                    fileStream.Dispose();

                    if (File.Exists(CachePath)) File.Delete(CachePath);
                    File.Move(CachePath + ".tmp", CachePath);
                }
                catch (Exception e)
                {
                    fileStream.Dispose();
                    Trace.WriteLine($"网络错误或资源 {FileName} 缓存写入失败. URL {url} 用时 {sw.Elapsed.TotalSeconds:F1}s", "WFResource");
                    Trace.WriteLine(e);
                    sw.Restart();

                    // 如果保存失败 可能是网络错误 或者文件无法写入 就直接再请求一次
                    await using var stream2 = await requester(url);
                    Value = await resourceLoader(stream2);
                    Trace.WriteLine($"资源 {FileName} 获取完成. URL {url} 用时 {sw.Elapsed.TotalSeconds:F1}s", "WFResource");
                    return;
                }

                // 这里保存到了缓存里 从缓存里读入
                await using var file = File.OpenRead(CachePath);
                Value = await resourceLoader(file);
                Trace.WriteLine($"资源 {FileName} 获取完成. URL {url} 用时 {sw.Elapsed.TotalSeconds:F1}s", "WFResource");
            }
            catch (Exception e)
            {
                Trace.WriteLine($"资源 {FileName} 载入失败. URL {url} 用时 {sw.Elapsed.TotalSeconds:F1}s", "WFResource");
                Trace.WriteLine(e);
                if (initTask == null)
                {
                    Trace.WriteLine("这个资源已经从缓存读入, 所以不会对运行造成影响, 但是这个资源可能不是最新.");
                }
                else
                {
                    if (!Program.DontThrowIfResourceUnableToLoad)
                        throw;

                    Trace.WriteLine("WFBot 不会停止运行, 但是功能可能无法正常运行.");
                }
            }
        }

        public async Task<Stream> RequestResourceFromTheWideWorldOfWeb(string urlp)
        {
            var httpClient = new HttpClient(new RetryHandler(new HttpClientHandler{AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Brotli}));
            httpClient.Timeout = Timeout.InfiniteTimeSpan;
            if (header != null)
            {
                foreach (string key in header)
                {
                    httpClient.DefaultRequestHeaders.Add(key, header[key]);
                }
            }

            var dataString = await httpClient.GetStreamAsync(urlp);
            return dataString;
        }

        void LoadFromTheWideWorldOfWebNonBlocking()
        {
#pragma warning disable 4014
            LoadFromTheWideWorldOfWeb();
#pragma warning restore 4014
        }

        async Task<bool> LoadCache()
        {
            try
            {
                if (!File.Exists(CachePath)) return false;
                await using var file = File.OpenRead(CachePath);
                Value = await resourceLoader(file);
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"资源 {FileName} 从缓存载入失败.", "WFResource");
                Trace.WriteLine(e);
                if (File.GetAttributes(CachePath).HasFlag(FileAttributes.ReadOnly))
                {
                    Trace.WriteLine($"请不要像个小屁孩一样把 cache 设置成只读, 如果你想用本地辞典, 可以在 {OfflineDir} 中放入同名文件.");
                }
                return false;
            }
        }

        async Task<bool> LoadFromLocal()
        {
            try
            {
                if (!File.Exists(LocalPath)) return false;
                Value = await resourceLoader(File.OpenRead(LocalPath));

                return true;
            }
            catch (Exception)
            {
                Trace.WriteLine($"资源 {FileName} 从本地载入失败.", "WFResource");
                throw;
            }
        }
    }

    public class RetryHandler : DelegatingHandler
    {
        // Strongly consider limiting the number of retries - "retry forever" is
        // probably not the most user friendly way you could respond to "the
        // network cable got pulled out."
        private const int MaxRetries = 3;

        public RetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        { }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (var i = 0; i < MaxRetries; i++)
            {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
            }

            return response;
        }
    }
}
