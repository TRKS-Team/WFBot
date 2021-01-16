using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GammaLibrary.Enhancements;
using Newtonsoft.Json;
using WFBot.Utils;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WFBot.Features.Resource
{
    public static class ResourceLoaders<T>
    {
        public static WFResourceLoader<T> JsonDotNetLoader = stream =>
        {
            return Task.Run(() =>
            {
                using (var sr = new StreamReader(stream))
                using (var reader = new JsonTextReader(sr))
                {
                    return new Newtonsoft.Json.JsonSerializer().Deserialize<T>(reader);
                }
            });
        };

        public static WFResourceLoader<T> SystemTextJsonLoader = stream => JsonSerializer.DeserializeAsync<T>(stream).AsTask();
    }

    public delegate Task<T> WFResourceLoader<T>(Stream data);
    public delegate Task<Stream> WFResourceRequester(string url);

    public static class WFResourceStatic
    {
        public static readonly ConcurrentDictionary<string, int> CategoryVersionDictionary = new ConcurrentDictionary<string, int>();
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
        const string OfflineDir = "WFOfflineResource";
        const string CacheDir = "WFCaches";


        static WFResource()
        {
            Directory.CreateDirectory(OfflineDir);
            Directory.CreateDirectory(CacheDir);
        }

        protected WFResource(string url = null, string category = null, string fileName = null,
            WebHeaderCollection header = null, WFResourceLoader<T> resourceLoader = null,
            WFResourceRequester wfResourceRequester = null)
        {
            resourceLoader = resourceLoader ?? ResourceLoaders<T>.JsonDotNetLoader;
            // 这写的太屎了
            fileName = fileName ?? (url ?? string.Empty).Split('/').Last().Split('?').First().Split("!").First();

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
                initTask = null;
            }
        }

        async Task LoadFromTheWideWorldOfWeb()
        {
            try
            {
                var stream = await requester(url);

                try
                {
                    await using var fileStream = File.OpenWrite(CachePath);
                    await stream.CopyToAsync(fileStream);
                }
                catch (Exception e)
                {
                    Trace.WriteLine($"网络错误或资源 {FileName} 缓存写入失败.", "WFResource");
                    Trace.WriteLine(e);

                    var stream2 = await requester(url);
                    Value = await resourceLoader(stream2);
                    return;
                }

                await using var file = File.OpenRead(CachePath);
                Value = await resourceLoader(file);
                Trace.WriteLine($"资源 {FileName} 获取完成.", "WFResource");
            }
            catch (Exception)
            {
                Trace.WriteLine($"必要资源 {FileName} 从广域网载入失败.", "WFResource");
                throw;
            }
        }

        public async Task<Stream> RequestResourceFromTheWideWorldOfWeb(string urlp)
        {
            var httpClient = new HttpClient(new RetryHandler(new HttpClientHandler()));
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
            LoadFromTheWideWorldOfWeb();
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
            for (int i = 0; i < MaxRetries; i++)
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
