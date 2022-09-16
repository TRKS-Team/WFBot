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
using System.Runtime.CompilerServices;
using System.Text;
using GammaLibrary.Extensions;
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


        public static unsafe void Load(bool forceCreate = false)
        {
            if (!Config.Instance.EnableCustomCommandContent || CustomCommandContentConfig.Instance.Content.IsNullOrWhiteSpace()) return;

            var syntaxTree = CSharpSyntaxTree.ParseText(CustomCommandContentConfig.Instance.Content);
            var dir = Path.GetDirectoryName(typeof(object).Assembly.Location);

            if (File.Exists("temp.dll") && (forceCreate || CacheInvalid()))
            {
                File.Delete("temp.dll");
            }

            if (File.Exists("temp.dll"))
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
                    File.WriteAllBytes("temp.dll", dllStream.ToArray());
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
            var bytes = File.ReadAllBytes("temp.dll");
            CustomCommandContentConfig.Save();
            var type = Assembly.Load(bytes).GetType("WFBot.Features.Utils.WFFormatterCustom").GetMethods(BindingFlags.Public | BindingFlags.Static);
            var originType = typeof(WFFormatter).GetMethods(BindingFlags.Public | BindingFlags.Static);

            foreach (var info in originType)
            {
                var handleValue = (int*)info.MethodHandle.Value;
                var method = type.FirstOrDefault(x => x.ToString().Replace("WFFormatterCustom", "WFFormatter") == info.ToString());
                if (method != null)
                {
                    //Console.WriteLine($"Replacing {method.ToString()}");
                    if (Environment.Is64BitProcess)
                    {
                        Unsafe.Write(handleValue + 4, method.MethodHandle.GetFunctionPointer().ToInt64());
                    }
                    else
                    {
                        Unsafe.Write(handleValue + 2, method.MethodHandle.GetFunctionPointer().ToInt32());
                    }
                }
            }



        }

        static bool CacheInvalid()
        {
            return CustomCommandContentConfig.Instance.LastCompileHash !=
                   CustomCommandContentConfig.Instance.Content.SHA256().ToHexString();

        }
    }
}
