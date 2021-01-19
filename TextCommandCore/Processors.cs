using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TextCommandCore
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
