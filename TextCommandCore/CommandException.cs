using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextCommandCore
{
    public class CommandException : Exception
    {
        public CommandException(string message) : base(message)
        {
        }
    }

    public class CommandMismatchException : Exception
    {
        public CommandMismatchException()
        {
        }
    }
}
