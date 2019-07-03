using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TRKS.WF.QQBot
{
    public abstract class Configuration<T> where T : Configuration<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
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
                    Instance = FileSystem.ReadFile(savePath).JsonDeserialize<T>();
                }
                else
                {
                    Instance = new T();
                    Save();
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e, nameof(Configuration<T>));
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Save() => Instance.ToJsonString().SaveToFile(SavePath);

        public static string SavePath => typeof(T).GetCustomAttribute<ConfigurationAttribute>().SaveName + ".json";
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

    public static class MessageExtensions
    {
        private static string platform => Config.Instance.Platform.ToString();

        public static string AddPlatformInfo(this string str)
        {
            return $"{str}\n\n数据来自[{platform} 平台]";
        }

        public static string AddHelpInfo(this string str)
        {
            return $"{str}\n可使用: /help来查看机器人的更多说明.";
        }

        public static string AddRemainCallCount(this string str, GroupNumber group)
        {
            return Config.Instance.CallperMinute == 0 || Messenger.GroupCallDic[group.ToString()] < 0 ? str : $"{str}\n本群每分钟还能调用{Config.Instance.CallperMinute - Messenger.GroupCallDic[group.ToString()]}次.";
        }
    }

}
