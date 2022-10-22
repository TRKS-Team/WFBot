using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using GammaLibrary.Enhancements;
using GammaLibrary.Extensions;
using Humanizer;
using Manganese.Array;
using Newtonsoft.Json;
using WFBot.Features.Common;
using WFBot.Features.Utils;
using WFBot.Utils;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Timer = System.Timers.Timer;

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
                if (str != string.Empty)
                    str = str[str.IndexOfAny(new[] { '{', '[' })..];

                return JsonConvert.DeserializeObject<T>(str, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

#else
                using var sr = new StreamReader(stream);
                int c;
                while (!((char)(c = sr.Peek()) == '[' || (char)c == '{' || c == -1)) sr.Read();
                using var reader = new JsonTextReader(sr);

                var serializer = new Newtonsoft.Json.JsonSerializer {NullValueHandling = NullValueHandling.Ignore};
                serializer.Error += (sender, args) =>
                {
                    if (args.ErrorContext.Error.Message.Contains("Error converting value {null} to type"))
                    {
                        args.ErrorContext.Handled = true;
                    }
                };
                return serializer
                    .Deserialize<T>(reader);
#endif
            });
        };

        public static WFResourceLoader<T> SystemTextJsonLoader = stream => JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: AsyncContext.GetCancellationToken()).AsTask();
    }

    public delegate Task<T> WFResourceLoader<T>(Stream data);
    public delegate Task<Stream> WFResourceRequester(string url);
    public delegate Task<bool> WFResourceUpdater<T>(WFResource<T> resource) where T : class;
    public delegate Task WFResourceFinisher();

    public static class WFResourceFinishers
    {
        public static Task UpdateWFTranslator()
        {
            WFResources.UpdateWFTranslator();
            return Task.CompletedTask;
        }

        public static Task UpdateWildcardSearcher()
        {
            WFResources.WildCardSearcher.UpdateSearcher();;
            return Task.CompletedTask;
        }

        public static Task UpdateTranslatorAndWildcardSearcher()
        {
            UpdateWFTranslator();
            UpdateWildcardSearcher();
            return Task.CompletedTask;
        }
    }
    public static class WFResourceUpdaters<T> where T : class
    {
        public static WFResourceUpdater<T> StringCompareDelegate = StringCompareUpdater;

        private static async Task<bool> StringCompareUpdater(WFResource<T> resource)
        {
            await using (var file = File.OpenRead(resource.CachePath))
            {
                await using (var stream = await resource.requester(resource.url))
                {
                    if (file.ReadToEnd() == stream.ReadToEnd()) return false;
                }
            }
            await resource.Reload();
            Messenger.SendDebugInfo($"资源系统: 正在刷新资源: {resource.FileName}");
            return true;
        }

        public static WFResourceUpdater<T> JsonStringCompareDelegate = JsonStringCompareUpdater;

        private static async Task<bool> JsonStringCompareUpdater(WFResource<T> resource)
        {
            await using (var file = File.OpenRead(resource.CachePath))
            {
                await using (var stream = await resource.requester(resource.url))
                {
                    if (JsonConvert.SerializeObject(resource.resourceLoader(file)) == JsonConvert.SerializeObject(resource.resourceLoader(stream)))
                    {
                        return false;
                    }
                }
            }
            await resource.Reload();
            Messenger.SendDebugInfo($"资源系统: 正在刷新资源: {resource.FileName}");
            return true;
        }

        public static WFResourceUpdater<T> JustUpdateDelegate = JustUpdateUpdater;

        private static async Task<bool> JustUpdateUpdater(WFResource<T> resource)
        {
            await resource.Reload();
            Trace.WriteLine($"资源系统: 正在刷新资源: {resource.FileName}");
            return true;
        }

        public static WFResourceUpdater<T> GitHubShaDelegate = GitHubSHAUpdater;

        private static async Task<bool> GitHubSHAUpdater(WFResource<T> resource)
        {
            try
            {
                var infos = GitHubInfos.Instance.Infos.Where(i => i.Category == resource.Category).ToList();

                if (!infos.Any()) return false;
                var info = infos.First();
                if (DateTime.Now - info.LastUpdated <= TimeSpan.FromMinutes(10)) return false;
                // 关于API的限制 有Token的话5000次/hr 无Token的话60次/hr 咱就不狠狠的撅GitHub的服务器了
                var sha = WFResources.GetSHA(info);
                if (sha == null) return false;
                if (info.SHA.IsNullOrEmpty())
                {
                    info.SHA = sha;
                    GitHubInfos.Save();
                    return false;
                }
                if (sha == info.SHA) return false;
                var s = $"发现{info.Name}有更新,正在更新···";
                if (Config.Instance.SendResourceUpdateNotification)
                {
                    Messenger.SendDebugInfo(s);
                }
                else
                {
                    Trace.WriteLine(s);
                }
                await Task.WhenAll(WFResourcesManager.WFResourceDic[info.Category].Select(r => r.Reload(false)));

                GitHubInfos.Instance.Infos.Where(i => i.Category == info.Category).ForEach(i =>
                {
                    i.LastUpdated = DateTime.Now;
                    i.SHA = sha;
                }).ToArray();
                GitHubInfos.Save();

                return true;
            }
            catch (Exception)
            {
                Trace.WriteLine("资源系统: 用于刷新资源文件的 GitHub Commits 获取失败, 这可能和网络有关系, 可以尝试阅读 https://github.com/TRKS-Team/WFBot/blob/universal/docs/token.md");
                return false;
            }
        }   
    }
    public static class WFResourceStatic
    {
        public static readonly ConcurrentDictionary<string, int> CategoryVersionDictionary =
            new ConcurrentDictionary<string, int>();
        internal static int ResourceCount;
        internal static int FinishedResourceCount;
    }

    public class WFResource<T> : IWFResource where T : class
    {
        volatile T value;
        readonly object locker = new object();
        public string url { get; }
        public readonly WFResourceLoader<T> resourceLoader;
        public string CachePath => Path.Combine(CacheDir, FileName);
        public string LocalPath => Path.Combine(OfflineDir, FileName);
        readonly WebHeaderCollection header;
        public const string OfflineDir = "WFOfflineResource";
        public const string CacheDir = "WFCaches";
        public bool UseAlternativeRequester { get; }
        bool RequestedRerequest { get; set; } = false;

        static WFResource()
        {
            Directory.CreateDirectory(OfflineDir);
            Directory.CreateDirectory(CacheDir);
        }

        protected WFResource(string url = null, string category = null, string fileName = null,
            WebHeaderCollection header = null, WFResourceLoader<T> resourceLoader = null,
            WFResourceRequester wfResourceRequester = null, WFResourceUpdater<T> updater = null, WFResourceFinisher finisher = null)
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
            this.updater = updater ?? WFResourceUpdaters<T>.StringCompareDelegate;
            this.finisher = finisher ?? WFResourceFinishers.UpdateWFTranslator;
            if (category != null && !WFResourceStatic.CategoryVersionDictionary.ContainsKey(category))
            {
                WFResourceStatic.CategoryVersionDictionary[category] = 0;
            }
            Version = 0;
        }

        public static WFResource<T> Create(string url = null, string category = null, string fileName = null,
            WebHeaderCollection header = null, WFResourceLoader<T> resourceLoader = null,
            WFResourceRequester requester = null, WFResourceUpdater<T> updater = null, WFResourceFinisher finisher = null)
        {
            var result = new WFResource<T>(url, category, fileName, header, resourceLoader, requester, updater, finisher);
            Interlocked.Increment(ref WFResourceStatic.ResourceCount);
            result.initTask = result.Reload(true);
            if (WFResourcesManager.WFResourceDic.ContainsKey(result.Category ?? ""))
            {
                WFResourcesManager.WFResourceDic[result.Category ?? ""].Add(result);
            }
            else
            {
                WFResourcesManager.WFResourceDic[result.Category ?? ""] = new List<IWFResource> {result};
            }
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
        public bool Reloading => _locker.CurrentCount == 0;
        public WFResourceRequester requester { get; set; }
        WFResourceUpdater<T> updater;
        WFResourceFinisher finisher;
        public bool IsGitHub
        {
            get => updater.Equals(WFResourceUpdaters<T>.GitHubShaDelegate);
        }

        public async Task Update() =>
            await Task.Run(async () =>
            {
                if (await updater(this))
                {
                    await finisher();
                }
            });

        public async Task Reload(bool isFirstTime = false)
        {
            using var resourceLock = WFBotResourceLock.Create($"资源刷新 {FileName}");
            try
            {
                await _locker.WaitAsync();
                if (await LoadFromLocal())
                {
                    if (isFirstTime) Trace.WriteLine($"资源系统: 资源 {FileName} 从本地载入.");
                    return;
                }

                if (isFirstTime && await LoadCache())
                {
                    Trace.WriteLine($"资源系统: 资源 {FileName} 从缓存载入.");
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
                    Interlocked.Increment(ref WFResourceStatic.FinishedResourceCount);
                    Console.WriteLine($"> 资源系统: 加载进程 {WFResourceStatic.FinishedResourceCount} / {WFResourceStatic.ResourceCount}.");
                }
            }
        }
        async Task LoadFromTheWideWorldOfWeb()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await using var stream = await requester(url);
                var fileStream = File.Open(CachePath + ".tmp", FileMode.Create, FileAccess.Write, FileShare.Read);
                var cts = new CancellationTokenSource();
                if (!RequestedRerequest)
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(60));
                }

                try
                {
                    // 尝试将资源保存到缓存再读取...
                    var copyTask = stream.CopyToAsync(fileStream, cts.Token);
                    while (true)
                    {
                        var waitTask = Task.Delay(3.Seconds());
                        await Task.WhenAny(copyTask, waitTask);
                        if (copyTask.IsCanceled)
                        {
                            throw new TaskCanceledException();
                        }
                        if (copyTask.IsCompleted) break;
                        Console.WriteLine($"- 资源 {FileName} 还没有下载完. 下载了 {fileStream.Length.Bytes().Megabytes:F2} MB.");
                    }

                    fileStream.Dispose();

                    if (File.Exists(CachePath)) File.Delete(CachePath);
                    File.Move(CachePath + ".tmp", CachePath);
                }
                catch (Exception e)
                {
                    fileStream.Dispose();
                    if (e is TaskCanceledException)
                    {
                        RequestedRerequest = true;
                        await LoadFromTheWideWorldOfWeb();
                        RequestedRerequest = false;
                        return;
                    }
                    Trace.WriteLine($"资源系统: 网络错误或资源 {FileName} 缓存写入失败. URL [{url}], 用时 {sw.Elapsed.TotalSeconds:F1}s.");
                    Trace.WriteLine(e);
                    sw.Restart();

                    // 如果保存失败 可能是网络错误 或者文件无法写入 就直接再请求一次
                    await using var stream2 = await requester(url);
                    Value = await resourceLoader(stream2);
                    Trace.WriteLine($"资源系统: 资源 {FileName} 获取完成. URL [{url}], 用时 {sw.Elapsed.TotalSeconds:F1}s.");
                    return;
                }

                // 这里保存到了缓存里 从缓存里读入
                await using var file = File.OpenRead(CachePath);
                Value = await resourceLoader(file);
                Trace.WriteLine($"资源系统: 资源 {FileName} 获取完成. URL [{url}], 用时 {sw.Elapsed.TotalSeconds:F1}s.");
            }
            catch (Exception e)
            {
                Trace.WriteLine($"资源系统: 资源 {FileName} 载入失败. URL [{url}], 用时 {sw.Elapsed.TotalSeconds:F1}s.");
                Trace.WriteLine(e);
                if (initTask == null)
                {
                    Trace.WriteLine("资源系统: 这个资源已经从缓存读入, 所以不会对运行造成影响, 但是这个资源可能不是最新.");
                }
                else
                {
                    if (!Program.DontThrowIfResourceUnableToLoad)
                        throw;

                    Trace.WriteLine("WFBot 不会停止运行, 但是功能可能无法正常运行.");
                }
            }
        }

        public async Task<Stream> RequestResourceFromTheWideWorldOfWeb(string url)
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

            var dataString = await httpClient.GetStreamAsync(url);
            return dataString;
        }
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(5);
        public async Task<Stream> JsDelivrWideWorldOfWebRequester(string url)
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

            Stream dataString;
            try
            {
                await semaphoreSlim.WaitAsync();
                if (RequestedRerequest)
                {
                    dataString = await httpClient.GetStreamAsync(url.Replace("https://cdn.jsdelivr.net/gh", "https://wfbot.kraber.top:8888/Resources"));
                    return dataString;
                }

                // 写下载超时的话
                // 1. 操作 HttpClient 实例的 TimeOut 并捕获 TaskCanceledException

                /*
                 *  var httpClient = new HttpClient();
                 *  httpClient.Timeout = TimeSpan.FromMilliseconds(1);
                 *  try
                 *  {
                 *      await httpClient.GetStringAsync("https://baidu.com");
                 *  }
                 *  catch (TaskCanceledException e)
                 *  {
                 *      Console.WriteLine(e);
                 *  }
                 *
                 */

                // 2. 使用 CancellationTokenSource, 传递 CancellationToken, 并捕获 TaskCanceledException

                /*
                 *  var httpClient = new HttpClient();
                 *  var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));
                 *  try
                 *  {
                 *      await httpClient.GetStringAsync("https://baidu.com", cts.Token);
                 *  }
                 *  catch (TaskCanceledException e)
                 *  {
                 *      Console.WriteLine(e);
                 *  }
                 *
                 */

                // 3. 使用 CancellationTokenSource, 在一段时间后检查任务是否完成, 若没有完成则手动调用其 Cancel 方法
                // 在这种情况下不会抛出异常, 但是在使用 `await task` 会抛出 TaskCanceledException 在调用 `task.Result` 时会抛出 AggregateException 
                // 同时也可以通过调用 task.Exception 来获取里面的异常

                /*
                 *  var httpClient = new HttpClient();
                 *  var cts = new CancellationTokenSource();
                 *
                 *  var task = httpClient.GetStringAsync("https://baidu.com", cts.Token);
                 *  await Task.Delay(TimeSpan.FromMilliseconds(1));
                 *  if (!task.IsCompleted)
                 *  {
                 *      // 这里 Cancel 不 Cancel 对结果没有影响, 但是调用会结束数据传输
                 *      cts.Cancel();
                 *      // do sth
                 *  }
                 *  else
                 *  {
                 *      // do sth
                 *  }
                 *
                 */

                // 另外其实理论上有一个小问题, 最好是一个程序只用一个 HttpClient, 在请求量较大时 new 很多的 HttpClient 会导致系统可用端口减少 (port exhaustion) 即 (TIME_WAIT)
                // 但是我们请求量比较小就懒得改了
                // 见 https://github.com/CurseForgeCommunity/.NET-APIClient/issues/1
                
                // ~~T-O-D-O~~ cY 帮我 改改 我 去睡大觉了
                // 我也去睡大觉咯


                // 但是我们这玩意写的是 GetStream。。 不能从这块下手 也就是上面写的都是无效的 得从调用 requester 那下手
                // try
                // {
                dataString = await httpClient.GetStreamAsync(url);
                return dataString;
                // }
                // catch (TaskCanceledException)
                // {
                //     Console.WriteLine("有一个或多个Jsdelivr资源请求超时, 将会更换全局下载源为WFBot镜像.");
                //     WFResources.IsJsDelivrFailed = true;
                //     dataString = await httpClient.GetStreamAsync(url.Replace("https://cdn.jsdelivr.net/gh", "https://wfbot.kraber.top:8888/Resources"));
                //     return dataString;
                // }
            }
            catch
            {
                Trace.WriteLine("资源系统: 有一个或多个Jsdelivr资源请求错误, 将会更换全局下载源为WFBot镜像.");
                dataString = await httpClient.GetStreamAsync(url.Replace("https://cdn.jsdelivr.net/gh",
                    "https://wfbot.kraber.top:8888/Resources"));
                return dataString;
            }
            finally
            {
                semaphoreSlim.Release();
            }
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
                Trace.WriteLine($"资源系统: 资源 {FileName} 从缓存载入失败.");
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
                Trace.WriteLine($"资源系统: 资源 {FileName} 从本地载入失败.");
                throw;
            }
        }
    }

    public class RetryHandler : DelegatingHandler
    {
        // Strongly consider limiting the number of retries - "retry forever" is
        // probably not the most user friendly way you could respond to "the
        // network cable got pulled out."
        private const int MaxRetries = 4;

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

                await Task.Delay(500);
            }

            return response;
        }
    }
}
