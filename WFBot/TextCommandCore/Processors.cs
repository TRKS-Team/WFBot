using System.Reflection;

namespace WFBot.TextCommandCore
{
    public interface IPreProcessor
    {
        string Process<T>(MethodInfo method, string msg, WFBot.TextCommandCore.ICommandHandler<T> handlers) where T : WFBot.TextCommandCore.ICommandHandler<T>;
    }

    public interface IPostProcessor
    {
        string Process<T>(MethodInfo method, string msg, string result, WFBot.TextCommandCore.ICommandHandler<T> handlers) where T : WFBot.TextCommandCore.ICommandHandler<T>;
    }
}
