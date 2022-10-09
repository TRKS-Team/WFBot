using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using GammaLibrary.Extensions;
using HarmonyLib;
using WFBot.Features.Utils;
using WFBot.Utils;

namespace WFBot.Features.CustomCommandContent
{
    [Configuration("CustomCommandContentConfig")]
    public class CustomCommandContentConfig : Configuration<CustomCommandContentConfig>
    {
        public string LastSaveVersion = "";
        public string Content = "";
        public string LastCompileHash = "";
    }

    public class CustomCommandContentHandler
    {
        static bool FirstLoad = true;

        public static unsafe void Load(bool forceCreate = false)
        {
            if (!Config.Instance.EnableCustomCommandContent || CustomCommandContentConfig.Instance.Content.IsNullOrWhiteSpace()) return;
            if (FirstLoad)
            {
                FirstLoad = false;
                new Harmony("what").PatchAll(Assembly.GetCallingAssembly());
            }
            var syntaxTree = CSharpSyntaxTree.ParseText(CustomCommandContentConfig.Instance.Content);
            var dir = Path.GetDirectoryName(typeof(object).Assembly.Location);

            var tempDll = "WFCaches/temp.dll";
            if (File.Exists(tempDll) && (forceCreate || CacheInvalid()))
            {
                File.Delete(tempDll);
            }

            if (File.Exists(tempDll))
            {
                goto load;
            }

            var compilation = CSharpCompilation.Create(
                "Formatter",
                new[] { syntaxTree },
                Directory.GetFiles(".", "*.dll").Concat(Directory.GetFiles(dir, "System.*").Where(x => !x.Contains("Native"))).Select(d => MetadataReference.CreateFromFile(d)).Concat(new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(dir, "mscorlib.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(dir, "netstandard.dll")),
                }),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


            using (var dllStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var emitResult = compilation.Emit(dllStream, pdbStream);
                if (!emitResult.Success)
                {
                    var errors = new StringBuilder();
                    errors.AppendLine("在编译自定义命令内容时出现了错误:");
                    foreach (var diagnostic in emitResult.Diagnostics)
                    {
                        errors.AppendLine($"{diagnostic}");
                        Trace.WriteLine(diagnostic);
                    }

                    throw new Exception(errors.ToString());
                    // emitResult.Diagnostics
                }
                else
                {
                    dllStream.Seek(0, SeekOrigin.Begin);
                    File.WriteAllBytes(tempDll, dllStream.ToArray());
                }
            }

            CustomCommandContentConfig.Instance.LastCompileHash =
                CustomCommandContentConfig.Instance.Content.SHA256().ToHexString();
            CustomCommandContentConfig.Instance.LastSaveVersion = WFBotCore.Version;
            CustomCommandContentConfig.Save();

            load:
            if (CustomCommandContentConfig.Instance.LastSaveVersion != WFBotCore.Version)
            {
                Console.WriteLine($"************警告: 自定义命令处理内容上次保存时的版本为 {CustomCommandContentConfig.Instance.LastSaveVersion} 可能有内容已经过期, 请查看 diff <https://github.com/TRKS-Team/WFBot/compare/v1.0.{CustomCommandContentConfig.Instance.LastSaveVersion.Split('.').Last().Split('+').First()}-universal.0...v1.0.{WFBotCore.Version.Split('.').Last().Split('+').First()}-universal.0?diff=unified>并修改.");
            }
            var bytes = File.ReadAllBytes(tempDll);
            CustomCommandContentConfig.Save();
            var type = Assembly.Load(bytes).GetType("WFBot.Features.Utils.WFFormatterCustom").GetMethods(BindingFlags.Public | BindingFlags.Static);
            var originType = typeof(WFFormatter).GetMethods(BindingFlags.Public | BindingFlags.Static);
            
            // foreach (var info in originType)
            // {
            //     var handleValue = (int*)info.MethodHandle.Value;
            //     var method = type.FirstOrDefault(x => x.ToString().Replace("WFFormatterCustom", "WFFormatter") == info.ToString());
            //     var m1 = new DynamicMethod("", info.ReturnType, info.GetParameters().Select(x=> x.ParameterType).ToArray());
            //     
            //     if (method != null)
            //     {
            //         //Console.WriteLine($"Replacing {method.ToString()}");
            //         if (Environment.Is64BitProcess)
            //         {
            //             Unsafe.Write(handleValue + 4, method.MethodHandle.GetFunctionPointer().ToInt64());
            //         }
            //         else
            //         {
            //             Unsafe.Write(handleValue + 2, method.MethodHandle.GetFunctionPointer().ToInt32());
            //         }
            //     }
            // }
            foreach (var info in type)
            {
                proxyMethods[info.ToString().Replace("WFFormatterCustom", "WFFormatter")] = info;
            }


        }

        static bool CacheInvalid()
        {
            return CustomCommandContentConfig.Instance.LastCompileHash !=
                   CustomCommandContentConfig.Instance.Content.SHA256().ToHexString();

        }

        static Dictionary<string, MethodBase> proxyMethods = new Dictionary<string, MethodBase>();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static object InvokeInternal(string methodInfoName, object[] arguments)
        {
            if (proxyMethods.ContainsKey(methodInfoName))
            {
                return proxyMethods[methodInfoName].Invoke(null, arguments);
            }

            return null;
        }
    }

    [HarmonyPatch()]
    public class CustomCommandContentCreator
    {
        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            var s = AccessTools.GetTypesFromAssembly(typeof(WFBotCore).Assembly).Where(t => t.Name == "WFFormatter")
                .SelectMany(type => type.GetMethods())
                .Where(m => m.Name != "GetType" && m.Name != "Equals" && m.Name != "GetHashCode")
                .Where(m => !(m.Name == "ToString" && m.GetParameters().Length == 0))
                .Cast<MethodBase>().ToArray();
           

            return s;
        }

        [HarmonyPrefix]
        static bool Prefix(object __instance, MethodBase __originalMethod, object[] __args, ref object __result)
        {
            var s = __originalMethod.ToString();
            __result = CustomCommandContentHandler.InvokeInternal(s, __args);
            return __result == null;
        }
    }
}
