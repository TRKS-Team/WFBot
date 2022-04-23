using System;

namespace WFBot.TextCommandCore
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
