using System.Reflection;

namespace WFBot.TextCommandCore
{
    public interface IPreProcessor
    {
        string Process<T>(MethodInfo method, string msg, ICommandHandler<T> handlers) where T : ICommandHandler<T>;
    }

    public interface IPostProcessor
    {
        string Process<T>(MethodInfo method, string msg, string result, ICommandHandler<T> handlers) where T : ICommandHandler<T>;
    }
}
