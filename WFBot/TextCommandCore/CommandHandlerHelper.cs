using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Fody;
using GammaLibrary.Extensions;
using WFBot.Orichalt;

[assembly: ConfigureAwait(false)]

namespace WFBot.TextCommandCore
{
    public static class CommandHandlerHelper
    {
        static readonly ConcurrentDictionary<Type, CommandInfo[]> CommandInfosCache = new ConcurrentDictionary<Type, CommandInfo[]>();
        public static int CommandExpectedTime = 12 * 1000;

        public static void InitCommandHandlerCollection<T>()
        {
            var type = typeof(T);
            if (CommandInfosCache.ContainsKey(type)) return;

            var infos = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(info => info.GetCustomAttributes().FirstOrDefault(attrib => attrib is MatcherAttribute) != null)
                .Select(info => new CommandInfo(info))
                .ToArray();
            CommandInfosCache[type] = infos;
        }

        public static CommandInfo[] GetCommandInfos<T>()
        {
            var type = typeof(T);
            if (!CommandInfosCache.ContainsKey(type)) InitCommandHandlerCollection<T>();

            return CommandInfosCache[type];
        }

        public static async Task<(bool matched, string result)> ProcessCommandInput<T>(this ICommandHandler<T> handlers) where T : ICommandHandler<T>
        {
            var message = handlers.Message.Trim();
            var o = handlers.O;
            if (message.IsNullOrEmpty()) return (false, null);

            string result;
            try
            {
                var method = GetCommandHandler<T>(message);
                message = PreProcess(method, message, handlers);

                var param = BuildParams(message, method);
                var needMeasureTime = !method.IsAttributeDefined<DoNotMeasureTimeAttribute>();

                Trace.WriteLine($"命令 {handlers.Message} 开始处理..");
                Task<string> task;
                if (method.ReturnType == typeof(Task))
                {
                    task = Task.Run(Helper);

                    async Task<string> Helper()
                    {
                        await (Task) method.Invoke(handlers, param);
                        return null;
                    }
                }
                else if (method.ReturnType == typeof(Task<string>))
                {
                    task = Task.Run(async () => await (Task<string>) method.Invoke(handlers, param));
                }
                else if (method.ReturnType == typeof(string) || method.ReturnType == typeof(void))
                {
                    task = Task.Run(() => method.Invoke(handlers, param) as string);
                }
                else
                {
                    throw new Exception($"无效方法参数: {method}");
                }

                var waitTask = Task.Delay(needMeasureTime ? CommandExpectedTime : -1);

                await Task.WhenAny(task, waitTask);
                if (waitTask.IsCompleted && !task.IsCompleted)
                    handlers.MessageSender("很抱歉, 这个命令可能需要更长的时间来执行. 请耐心等待.");
                try
                {
                    result = await task;
                }
                catch (Exception e)
                {
                    throw new TargetInvocationException(e);
                }

                result = PostProcess(method, message, result, handlers);
            }
            catch (CommandMismatchException)
            {
                return (false, null);
            }
            catch (CommandException e)
            {
                result = e.Message;
            }
            catch (AggregateException e)
            {
                result = $"请将下面的消息汇报给管理员: {e}";
            }
            catch (OperationCanceledException e)
            {
                result = $"操作超时: {e}";
            }
            catch (Exception e)
            {
                do
                {
                    var innerException = !(e is AggregateException) && !(e is TargetInvocationException) ? e : Unwrap(e);

                    Exception Unwrap(Exception e1)
                    {
                        while (true)
                        {
                            var e2 = e1 switch
                            {
                                AggregateException a => a.Flatten().InnerExceptions.FirstOrDefault(),
                                TargetInvocationException t => t.InnerException,
                                _ => throw new Exception("啥玩意啊")
                            };
                            if (e2 is AggregateException || e2 is TargetInvocationException)
                            {
                                e1 = e2;
                                continue;
                            }

                            return e2;
                        }
                    }

                    switch (innerException)
                    {
                        case CommandException _:
                            result = innerException.Message;
                            break;
                        case CommandMismatchException _:
                            return (false, null);
                        case TaskCanceledException _:
                        case OperationCanceledException _:
                        case TimeoutException _:
                            result = $"操作超时: {handlers.Message}";
                            break;
                        case HttpRequestException _:
                            result = $"网络请求错误: ";
                            break;
                        case NullReferenceException _:
                            result = "发生异常: 找不到对象.";
                            handlers.ErrorMessageSender($"在处理 {o.GetInfo()} 的命令时发生问题.\n" +
                                                        $"命令内容为 [{message}].\n" +
                                                        $"异常信息:\n" +
                                                        $"{innerException}");
                            break;
                        default:
                            result = $"发生异常: {innerException?.Message}";
                            handlers.ErrorMessageSender($"在处理 {o.GetInfo()} 的命令时发生问题.\n" +
                                                        $"命令内容为 [{message}].\n" +
                                                        $"异常信息:\n" +
                                                        $"{innerException}");
                            break;
                    }
                } while (false);
            }

            if (!result.IsNullOrWhiteSpace())
                handlers.MessageSender(result);
            return (true, result);
        }

        static string PreProcess<T>(MethodInfo method, string message, ICommandHandler<T> handlers) where T : ICommandHandler<T>
        {
            var result = method.GetCustomAttributes().OfType<IPreProcessor>().Aggregate(message,
                (current, preProcessor) => preProcessor.Process(method, current, handlers));
            if (handlers is CommandHandlerBase<T> handler) handler.OnProcessingMessage();

            return result;
        }

        static string PostProcess<T>(MethodInfo method, string message, string result,
            ICommandHandler<T> handlers) where T : ICommandHandler<T>
        {
            if (handlers is CommandHandlerBase<T> handler) handler.OnProcessedMessage();

            return method.GetCustomAttributes().OfType<IPostProcessor>().Aggregate(result, (current, processor) => processor.Process(method, message, current, handlers));
        }

        static object[] BuildParams(string message, MethodInfo method)
        {
            if (method.IsAttributeDefined<CombineParamsAttribute>()) return GetCombinedParams(message, method);

            var requiredParams = method.GetParameters();
            var providedParams = message.Split(' ').Skip(1).ToArray();
            var resultParams = new object[requiredParams.Length];

            var minRequiredParams = requiredParams.Count(info => !info.HasDefaultValue);
            var maxRequiredParams = requiredParams.Length;

            if (providedParams.Length < minRequiredParams) throw new CommandException("参数过少");
            var delta = requiredParams.Length - providedParams.Length;

            if (method.IsAttributeDefined<CombineStartAttribute>())
                providedParams = CombineStart(providedParams, requiredParams, delta);
            if (method.IsAttributeDefined<CombineEndAttribute>())
                providedParams = CombineEnd(providedParams, requiredParams);

            if (providedParams.Length > maxRequiredParams && maxRequiredParams != 0) throw new CommandException("参数过多");

            for (var index = 0; index < requiredParams.Length; index++)
            {
                var requiredParam = requiredParams[index];
                var providedParam = providedParams.SafeGet(index);

                if (providedParam == null)
                {
                    resultParams[index] = requiredParam.DefaultValue;
                }
                else
                {
                    resultParams[index] = GetParam(providedParam, requiredParam.ParameterType);
                }
            }

            return resultParams;
        }

        static string[] CombineEnd(string[] providedParams, ParameterInfo[] requiredParams)
        {
            var queue = new Queue<string>(providedParams);
            if (requiredParams.Any(p => p.HasDefaultValue)) throw new Exception("定义真牛逼.");

            providedParams = new string[requiredParams.Length];
            for (var i = 0; i < requiredParams.Length - 1; i++)
            {
                providedParams[i] = queue.Dequeue();
            }

            providedParams[providedParams.Length - 1] = queue.Connect(" ");
            return providedParams;
        }

        static string[] CombineStart(string[] providedParams, ParameterInfo[] requiredParams, int delta)
        {
            var stack = new Stack<string>(providedParams);
            providedParams = new string[requiredParams.Length - delta];
            for (var i = providedParams.Length - 1; i > 0; i--)
            {
                providedParams[i] = stack.Pop();
            }

            providedParams[0] = stack.Reverse().Connect(" ");
            return providedParams;
        }

        static object[] GetCombinedParams(string message, MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != 1) throw new Exception("使用 CombinedParams 时的参数数量只能有一个, 且必须为 string 类型.");
            var param = parameters[0];

            var result = message.Split(' ').Skip(1).Connect(" ");
            if (!result.IsNullOrEmpty())
                return ((object)result).AsArray();

            if (param.HasDefaultValue)
                return param.DefaultValue.AsArray();

            throw new CommandException("参数过少");
        }

        static object GetParam(string providedParam, Type requiredParamParameterType)
        {
            if (requiredParamParameterType == typeof(string))
            {
                return providedParam;
            }

            if (requiredParamParameterType == typeof(BigInteger))
            {
                if (!BigInteger.TryParse(providedParam, out var num)) Throw();

                return num;
            }

            if (requiredParamParameterType == typeof(int))
            {
                if (!int.TryParse(providedParam, out var num)) Throw();

                return num;
            }

            if (requiredParamParameterType == typeof(long))
            {
                if (!long.TryParse(providedParam, out var num)) Throw();

                return num;
            }

            if (requiredParamParameterType == typeof(double))
            {
                if (!double.TryParse(providedParam, out var num)) Throw();

                return num;
            }

            throw new Exception("LG 害人不浅.");

            void Throw()
            {
                throw new CommandException("您参数真牛逼. (不是数字)");
            }
        }

        static MethodInfo GetCommandHandler<T>(string message)
        {
            var 我不知道该咋命名了 = message.Split(' ')[0];
            var matchInfo = GetCommandInfos<T>().FirstOrDefault(info => info.Matchers.Any(m => m(我不知道该咋命名了)));
            if (matchInfo is null) throw new CommandMismatchException();

            return matchInfo.Method;
        }

        static T SafeGet<T>(this T[] array, int position) where T : class
        {
            return position >= array.Length ? null : array[position];
        }
    }
}