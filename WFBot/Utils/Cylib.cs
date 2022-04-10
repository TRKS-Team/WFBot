using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using WFBot.Features.Utils;
using WFBot.Orichalt;

namespace WFBot.Utils
{
    public abstract class Configuration<T> where T : Configuration<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (_instance == null) Update();
                return _instance;
            }
            protected set => _instance = value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Update()
        {
            try
            {
                var savePath = SavePath;
                if (FileSystem.Exists(savePath))
                {
                    _instance = FileSystem.ReadFile(savePath).JsonDeserialize<T>();
                }
                else
                {
                    _instance = new T();
                    Save();
                }

                if (_instance == null)
                {
                    throw new Exception("有些东西出了点问题.");
                }
                Instance.AfterUpdate();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e, nameof(Configuration<T>));
                throw;
            }
        }

        protected virtual void AfterUpdate()
        {
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Save() => Instance.ToJsonString().SaveToFile(SavePath);

        public static string SavePath
        {
            get
            {
                var basePath = typeof(T).GetCustomAttribute<ConfigurationAttribute>().SaveName + ".json";
                return Program.UseConfigFolder ? Path.Combine("WFBotConfigs", basePath) : basePath;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ConfigurationAttribute : Attribute
    {
        public string SaveName { get; }

        public ConfigurationAttribute(string saveName)
        {
            SaveName = saveName;
        }
    }

    public static class FileSystem
    {
        public static string ReadFile(string path)
        {
            return File.ReadAllText(GetPath(path));
        }

        public static bool Exists(string path)
        {
            return File.Exists(GetPath(path));
        }

        public static string GetDirectoryPath(string path)
        {
            return Path.GetDirectoryName(GetPath(path));
        }

        public static void EnsureDirectoryExists(string path)
        {
            var dire = GetDirectoryPath(path);
            if (!Directory.Exists(dire)) Directory.CreateDirectory(dire);
        }

        public static void SaveToFileIfNotExists(this byte[] bytes, string path)
        {
            File.WriteAllBytes(GetPath(path), bytes);
        }

        public static void SaveToFile(this string str, string path)
        {
            File.WriteAllText(GetPath(path), str, Encoding.UTF8);
        }

        public static string GetPath(string path)
        {
            return path;
        }
    }
    public static class JsonExtensions
    {
        private static readonly SerializeSettings SerializeSettings = new SerializeSettings();

        public static string ToJsonString<T>(this T source)
        {
            return JsonConvert.SerializeObject(source, SerializeSettings);
        }

        public static T JsonDeserialize<T>(this string source)
        {
            return JsonConvert.DeserializeObject<T>(source, SerializeSettings);
        }

        public static string ToJsonString<T>(this T source, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(source, settings);
        }

        public static T JsonDeserialize<T>(this string source, JsonSerializerSettings settings)
        {
            return JsonConvert.DeserializeObject<T>(source, settings);
        }
    }

    public static class DictionaryExtensions
    {
        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> directory, TKey key, Func<TValue> creator = null) where TValue : new()
        {
            if (!directory.ContainsKey(key)) directory[key] = creator == null ? new TValue() : creator();
            return directory[key];
        }
    }

    public class SerializeSettings : JsonSerializerSettings
    {
        public SerializeSettings()
        {
            NullValueHandling = NullValueHandling.Include;
            Formatting = Formatting.Indented;
            MissingMemberHandling = MissingMemberHandling.Ignore;
        }
    }

    public static class SomeExtensions
    {
        public static StringBuilder TrimEnd(this StringBuilder sb)
        {
            if (sb == null || sb.Length == 0) return sb;

            int i = sb.Length - 1;

            for (; i >= 0; i--)
                if (!char.IsWhiteSpace(sb[i]))
                    break;

            if (i < sb.Length - 1)
                sb.Length = i + 1;

            return sb;
        }
    }

    public static class MessageExtensions
    {
        private static string platform => Config.Instance.Platform.ToString();

        public static string AddPlatformInfo(this string str)
        {
            return $"{str.TrimEnd()}\n\n数据来自[{platform} 平台]";
        }

        public static string AddHelpInfo(this string str)
        {
            return $"{str}\n可使用: /help来查看机器人的更多说明.";
        }

        public static StringBuilder AddPlatformInfo(this StringBuilder str)
        {
            str.TrimEnd();
            str.Append($"\n\n数据来自[{platform} 平台]");
            return str;
        }

        // unused
        public static StringBuilder AddHelpInfo(this StringBuilder str)
        {
            str.Append("\n可使用: /help来查看机器人的更多说明.");
            return str;
        }

        public static string AddRemainCallCount(this string str)
        {
            return AddRemainCallCount(str.TrimEnd(), AsyncContext.GetOrichaltContext());
        }

        public static StringBuilder AddRemainCallCount(this StringBuilder str)
        {
            AddRemainCallCount(str.TrimEnd(), AsyncContext.GetOrichaltContext());
            return str;
        }

        public static string AddRemainCallCount(this string str, OrichaltContext o)
        {
            switch (o.Platform)
            {
                case MessagePlatform.OneBot:
                    var context = MiguelNetwork.OrichaltContextManager.GetOneBotContext(o);
                    var group = context.Group;
                    if (Config.Instance.CallperMinute == 0 || MiguelNetwork.OneBotGroupCallDic[group] < 0)
                    {
                        return str;
                    }

                    var remainCount = Config.Instance.CallperMinute - MiguelNetwork.OneBotGroupCallDic[group];

                    if (remainCount > 0)
                    {
                        return $"{str}\n机器人在本群一分钟内还能发送{remainCount}条消息.";
                    }
                    else
                    {
                        return $"{str}\n机器人在本群一分钟内信息发送配额已经用完.";
                    }
                default:
                    return "";
            }
        }

        public static void AddRemainCallCount(this StringBuilder str, OrichaltContext o)
        {
            switch (o.Platform)
            {
                case MessagePlatform.OneBot:
                    var context = MiguelNetwork.OrichaltContextManager.GetOneBotContext(o);
                    var group = context.Group;
                    if (Config.Instance.CallperMinute == 0 || MiguelNetwork.OneBotGroupCallDic[group] < 0)
                    {
                        return;
                    }

                    var remainCount = Config.Instance.CallperMinute - MiguelNetwork.OneBotGroupCallDic[group];

                    if (remainCount > 0)
                    {
                        str.Append($"\n机器人在本群一分钟内还能发送{remainCount}条消息.");
                    }
                    else
                    {
                        str.Append($"\n机器人在本群一分钟内信息发送配额已经用完.");
                    }
                    break;
            }
        }
    }

}
