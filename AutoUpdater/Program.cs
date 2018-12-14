using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newbe.Mahua.Internals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WTF;

// ReSharper disable PossibleNullReferenceException

namespace AutoUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            var platformNames = new[] { "CleverQQ", "MPQ", "CQP", "QQLight" };
            foreach (var name in platformNames)
            {
                if (Path.GetFileNameWithoutExtension(name)
                    .Equals(MahuaPlatformValueProvider.CurrentPlatform.Value.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    var webClient = new WebClientEx(new CookieContainer());
                    if (File.Exists(name)) File.Delete(name);
                    webClient.Headers.Add("Authorization", "Bearer 6k1w2i924vgqpylm547l");
                    webClient.Headers.Add("Content-Type", "application/json");
                    webClient.Encoding = Encoding.UTF8;
                    var content = webClient.DownloadString("https://ci.appveyor.com/api/projects/TRKS-Team/WFBot");
                    dynamic jsonInfo =
                        (JObject)JsonConvert.DeserializeObject(content);
                    var value = webClient.DownloadString(
                        $"https://ci.appveyor.com/api/buildjobs/{jsonInfo.build.jobs[0].jobId.Value}/artifacts");
                    dynamic jsonArt = new JArray(JsonConvert.DeserializeObject(value));
                    foreach (dynamic art in jsonArt[0])
                    {
                        string n = art.fileName;
                        if (Path.GetFileNameWithoutExtension(n).Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            a:
                            try
                            {
                                webClient.DownloadFile($"https://ci.appveyor.com/api/buildjobs/{jsonInfo.build.jobs[0].jobId.Value}/artifacts/{art.fileName.Value}", name);
                                
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                goto a;
                            }
                            //Directory.Delete("YUELUO", true);
                            Unzip(ZipFile.OpenRead(name));
                        }
                    }
                }
            }
        }

        private static void Unzip(ZipArchive archive)
        {
            Directory.CreateDirectory("YUELUO");
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (!entry.FullName.Contains("YUELUO")) continue;
                
                var fullPath = Path.GetFullPath(entry.FullName);
                if (Path.GetFileName(fullPath).Length == 0)
                {
                    Directory.CreateDirectory(fullPath);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    entry.ExtractToFile(fullPath, true);
                }
            }
        }
    }
    public class WebClientEx : WebClient
    {
        public WebClientEx(CookieContainer container)
        {
            this.container = container;
        }

        public CookieContainer CookieContainer
        {
            get { return container; }
            set { container = value; }
        }

        private CookieContainer container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null)
            {
                request.CookieContainer = container;
            }
            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                container.Add(cookies);
            }
        }
    }
}
