using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Settings;
using TextCommandCore;
using TRKS.WF.QQBot.MahuaEvents;

namespace TRKS.WF.QQBot
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequireAdminAttribute : Attribute, IPreProcessor
    {
        public string Process<T>(MethodInfo method, string msg, ICommandHandler<T> handlers) where T : ICommandHandler<T>
        {
            if (handlers is ISender s && s.Sender.QQ != Config.Instance.QQ) throw new CommandException("你不是管理. (嫌弃脸)");

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
}
