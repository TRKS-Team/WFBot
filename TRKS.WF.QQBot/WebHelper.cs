using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TRKS.WF.QQBot
{
    public static class WebHelper
    {
        static WebHelper()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private static ThreadLocal<WebClient> webClient = new ThreadLocal<WebClient>(() => new WebClientEx2());
        public static T DownloadJson<T>(string url)
        {
            var count = 3;
            while (count --> 0)
            {
                try
                {
                    return webClient.Value.DownloadString(url).JsonDeserialize<T>();
                }
                catch (Exception)
                {
                }
            }
            throw new WebException($"在下载[{url}]时多次遇到问题. 请检查你的网络是否正常或联系项目负责人.");
        }

        public static T DownloadJson<T>(string url, WebHeaderCollection header)
        {
            var wc = new WebClientEx2();
            wc.Headers = header;
            return wc.DownloadString(url).JsonDeserialize<T>();
        }

        public static T UploadJson<T>(string url, string body)
        {
            return webClient.Value.UploadString(url, body).JsonDeserialize<T>();
        }

        public static T UploadJson<T>(string url, string body, WebHeaderCollection header)
        {
            var wc = new WebClientEx2();
            wc.Headers = header;
            return wc.UploadString(url, body).JsonDeserialize<T>();
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
