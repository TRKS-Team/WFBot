using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace WFBot.Utils
{
    // http://blog.flimflan.com/ASimpleColorConsoleTraceListener.html
    public class ColorConsoleTraceListener : ConsoleTraceListener
    {
        Dictionary<TraceEventType, ConsoleColor> eventColor = new Dictionary<TraceEventType, ConsoleColor>();

        public ColorConsoleTraceListener()
        {
            eventColor.Add(TraceEventType.Verbose, ConsoleColor.DarkGray);
            eventColor.Add(TraceEventType.Information, ConsoleColor.Gray);
            eventColor.Add(TraceEventType.Warning, ConsoleColor.Yellow);
            eventColor.Add(TraceEventType.Error, ConsoleColor.DarkRed);
            eventColor.Add(TraceEventType.Critical, ConsoleColor.Red);
            eventColor.Add(TraceEventType.Start, ConsoleColor.DarkCyan);
            eventColor.Add(TraceEventType.Stop, ConsoleColor.DarkCyan);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            TraceEvent(eventCache, source, eventType, id, "{0}", message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = getEventColor(eventType, originalColor);
            base.TraceEvent(eventCache, source, eventType, id, format, args);
            Console.ForegroundColor = originalColor;
        }

        private ConsoleColor getEventColor(TraceEventType eventType, ConsoleColor defaultColor)
        {
            if (!eventColor.ContainsKey(eventType))
            {
                return defaultColor;
            }
            return eventColor[eventType];
        }

    }
}
