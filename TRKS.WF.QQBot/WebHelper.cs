using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
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

        private static ThreadLocal<WebClient> webClient = new ThreadLocal<WebClient>(() => new WebClientEx2() { Encoding = Encoding.UTF8 });
        public static T DownloadJson<T>(string url)
        {
            while (true)
            {
                try
                {
                    return webClient.Value.DownloadString(url).JsonDeserialize<T>();
                }
                catch (Exception)
                {
                                 
                }
            }

        }

        public static T DownloadJson<T>(string url, WebHeaderCollection header)
        {
            var wc = webClient;
            wc.Value.Headers = header;
            return wc.Value.DownloadString(url).JsonDeserialize<T>();
        }


        public static T UploadJson<T>(string url, string body)
        {
            return webClient.Value.UploadString(url, body).JsonDeserialize<T>();
        }
        public static T UploadJson<T>(string url, string body, WebHeaderCollection header)
        {
            var wc = webClient;
            wc.Value.Headers = header;
            return wc.Value.UploadString(url, body).JsonDeserialize<T>();
        }
        public static void DowloadFile(string url, string path, string name)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
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
