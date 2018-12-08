using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newbe.Mahua.Internals;
using TRKS.WF.QQBot;
// ReSharper disable PossibleNullReferenceException

namespace AutoUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = ReleaseGetter.Get();
            foreach (var asset in data.assets)
            {
                if (Path.GetFileNameWithoutExtension(asset.name)
                    .Equals(MahuaPlatformValueProvider.CurrentPlatform.Value.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    var webClient = new WebClient();
                    if (File.Exists(asset.name)) File.Delete(asset.name);
                    webClient.DownloadFile(asset.browser_download_url, asset.name);
                    Directory.Delete("YUELUO", true);
                    Unzip(ZipFile.OpenRead(asset.name));
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
}
