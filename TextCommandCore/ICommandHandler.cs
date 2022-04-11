using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TextCommandCore
{
    public interface ICommandHandler<T> where T : ICommandHandler<T>
    {
        Action<Message> ErrorMessageSender { get; }
        string Message { get; }
    }

    public abstract class CommandHandlerBase<T> : ICommandHandler<T> where T : ICommandHandler<T>
    {
        public abstract Action<Message> ErrorMessageSender { get; }
        public abstract string Message { get; }

        public virtual void OnProcessingMessage() { }

        public virtual void OnProcessedMessage() { }
    }

    public class CommandInfo
    {
        public List<Predicate<string>> Matchers { get; }
        public MethodInfo Method { get; }

        public CommandInfo(MethodInfo method)
        {
            Matchers = method.GetCustomAttributes<MatcherAttribute>().Select(attrib => attrib.Matcher).ToList();
            Method = method;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MatchersAttribute : MatcherAttribute
    {
        public string[] Matchers { get; }

        public MatchersAttribute(params string[] matchers) : base(msg => matchers.Any(match => match == msg))
        {
            Matchers = matchers;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MatchersIgnoreCaseAttribute : MatcherAttribute
    {
        public string[] Matchers { get; }

        public MatchersIgnoreCaseAttribute(params string[] matchers) : base(msg => matchers.Any(match => match.Equals(msg, StringComparison.OrdinalIgnoreCase)))
        {
            Matchers = matchers;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class MatcherAttribute : Attribute
    {
        public Predicate<string> Matcher { get; }

        public MatcherAttribute(Predicate<string> matcher)
        {
            Matcher = matcher;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CombineParamsAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CombineStartAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CombineEndAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DoNotMeasureTimeAttribute : Attribute
    {
    }
    //TODO Timer' 请求比预计时间长
}
