using System;
using System.IO;
using System.Linq;
// code from https://github.com/newbe36524/Newbe.Mahua.Framework/blob/08808356bf4387c010a021520c7e2ea4e09a868a/src/Newbe.Mahua/MahuaPlatform.cs
namespace Newbe.Mahua.Internals
{
    /// <summary>
    /// 机器人平台
    /// </summary>
    public enum MahuaPlatform
    {
        /// <summary>
        /// 酷Q
        /// </summary>
        Cqp,

        /// <summary>
        /// MyPCQQ
        /// </summary>
        Mpq,

        /// <summary>
        /// Amanda
        /// </summary>
        Amanda,

        /// <summary>
        /// CleverQQ
        /// </summary>
        CleverQQ,

        /// <summary>
        /// QqLight
        /// </summary>
        QqLight,

    }
    internal interface IPlatformResolver
    {
        MahuaPlatform MahuaPlatform { get; }

        bool IsThis();
    }
    /// <summary>
    /// 当前插件平台信息读取
    /// </summary>
    public static class MahuaPlatformValueProvider
    {
        public static readonly Lazy<MahuaPlatform> CurrentPlatform =
            new Lazy<MahuaPlatform>(() =>
            {
                var mahuaPlatform = new IPlatformResolver[]
                    {
                        new CqpPlatformResolver(),
                        new MpqPlatformResolver(),
                        new AmandaPlatformResolver(),
                        new QqLightPlatformResolver(),
                        new CleverQqPlatformResolver()
                    }
                    .FirstOrDefault(x => x.IsThis())?.MahuaPlatform;
                if (mahuaPlatform == null)
                {
                    throw new Exception();
                }
                return mahuaPlatform.Value;
            });

        private static string GetCurrentDir() => Environment.CurrentDirectory;

        #region IPlatformResolver

        private class CqpPlatformResolver : IPlatformResolver
        {
            public MahuaPlatform MahuaPlatform { get; } = MahuaPlatform.Cqp;

            public bool IsThis()
            {
                var currentDir = GetCurrentDir();
                return File.Exists(Path.Combine(currentDir, "CQA.exe")) ||
                       File.Exists(Path.Combine(currentDir, "CQP.exe"));
            }
        }

        private class MpqPlatformResolver : IPlatformResolver
        {
            public MahuaPlatform MahuaPlatform { get; } = MahuaPlatform.Mpq;

            public bool IsThis()
            {
                var currentDir = GetCurrentDir();
                return File.Exists(Path.Combine(currentDir, "Core.exe"));
            }
        }

        private class AmandaPlatformResolver : IPlatformResolver
        {
            public MahuaPlatform MahuaPlatform { get; } = MahuaPlatform.Amanda;

            public bool IsThis()
            {
                var currentDir = GetCurrentDir();
                return File.Exists(Path.Combine(currentDir, "Amanda.exe"));
            }
        }

        private class CleverQqPlatformResolver : IPlatformResolver
        {
            public MahuaPlatform MahuaPlatform { get; } = MahuaPlatform.CleverQQ;

            public bool IsThis()
            {
                var currentDir = GetCurrentDir();
                return File.Exists(Path.Combine(currentDir, "CleverQQ Pro.exe")) ||
                       File.Exists(Path.Combine(currentDir, "CleverQQ Air.exe"));
            }
        }

        private class QqLightPlatformResolver : IPlatformResolver
        {
            public MahuaPlatform MahuaPlatform { get; } = MahuaPlatform.QqLight;

            public bool IsThis()
            {
                var currentDir = GetCurrentDir();
                return File.Exists(Path.Combine(currentDir, "QQLight.exe"));
            }
        }

        #endregion
    }
}