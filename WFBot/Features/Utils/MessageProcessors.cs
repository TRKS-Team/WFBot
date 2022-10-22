using GammaLibrary.Extensions;
using System;
using System.Linq;
using System.Reflection;
using WFBot.Events;
using WFBot.Features.Commands;
using WFBot.TextCommandCore;
using WFBot.Utils;

namespace WFBot.Features.Utils
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequireAdminAttribute : Attribute, IPreProcessor
    {
        public string Process<T>(MethodInfo method, string msg, ICommandHandler<T> handlers) where T : ICommandHandler<T>
        {
            if (handlers is ISender s && s.Sender.ToString() != Config.Instance.QQ) throw new CommandException("你不是管理. (嫌弃脸)");

            return msg;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequireCodeAttribute : Attribute, IPreProcessor
    {
        public string Process<T>(MethodInfo method, string msg, ICommandHandler<T> handlers) where T : ICommandHandler<T>
        {
            var split = msg.Split(' ');
            if (split.Length != 2) throw new CommandException("无效参数.");

            if (split.Last() == Config.Instance.Code)
            {
                return split.First();
            }

            throw new CommandException("口令无效");
        }
    }
    // 不写了 我去玩会destiny2 sb
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequireContainsCodeAttribute : Attribute, IPreProcessor
    {
        // TODO PreProcessor in Parameters.
        public string Process<T>(MethodInfo method, string msg, ICommandHandler<T> handlers) where T : ICommandHandler<T>
        {
            if (msg.Contains(Config.Instance.Code)) return msg;

            throw new CommandException("口令无效");
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SaveConfigAttribute : Attribute, IPostProcessor
    {
        public string Process<T>(MethodInfo method, string msg, string result, ICommandHandler<T> handlers) where T : ICommandHandler<T>
        {
            Config.Save();
            return result;
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class AddPlatformInfoAndAddRemainCallCountToTheCommandResultAndMakeTRKSHappyByDoingSoWhatSoEverAttribute : Attribute, IPostProcessor
    {
        public string Process<T>(MethodInfo method, string msg, string result, ICommandHandler<T> handlers) where T : ICommandHandler<T>
        {
            var handler = (CommandsHandler)handlers;
            if (AsyncContext.GetUseImageRendering()) return null;
            
            if (handler.OutputStringBuilder.IsValueCreated)
            {
                handler.OutputStringBuilder.Value.AddPlatformInfo().AddRemainCallCount();
            }

            return result.IsNullOrEmpty() ? null : result.AddPlatformInfo().AddRemainCallCount();
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class AddRemainCallCountAttribute : Attribute, IPostProcessor
    {
        public string Process<T>(MethodInfo method, string msg, string result, ICommandHandler<T> handlers) where T : ICommandHandler<T>
        {
            var handler = (CommandsHandler)handlers;
            if (handler.OutputStringBuilder.IsValueCreated)
            {
                handler.OutputStringBuilder.Value.AddRemainCallCount();
            }

            return result.IsNullOrEmpty() ? null : result.AddRemainCallCount();
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class AddPlatformInfoAttribute : Attribute, IPostProcessor
    {
        public string Process<T>(MethodInfo method, string msg, string result, ICommandHandler<T> handlers) where T : ICommandHandler<T>
        {
            var handler = (CommandsHandler)handlers;
            if (handler.OutputStringBuilder.IsValueCreated)
            {
                handler.OutputStringBuilder.Value.TrimEnd().AddPlatformInfo();
            }

            return result.IsNullOrEmpty() ? null : result.AddPlatformInfo();
        }
    }

}
