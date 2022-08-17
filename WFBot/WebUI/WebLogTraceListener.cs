using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace WFBot.WebUI
{
    public class WebLogTraceListener : TraceListener
    {
        public static FixedSizedQueue<string> Lines = new(1000);
        StringBuilder tempStringBuilder = new();

        public override void Write(string message)
        {
            lock (this)
            {
                foreach (var c in (message ?? "").ToCharArray())
                {
                    tempStringBuilder.Append(c);
                    if (c == '\n')
                    {
                        Lines.Enqueue(tempStringBuilder.ToString());
                        tempStringBuilder.Clear();
                        WFBotWebUIServer.NotifyDataChanged();
                    }
                }
            }
        }

        public override void WriteLine(string message)
        {
            Write($"{message}" + Environment.NewLine);
        }
    }

    
    // https://stackoverflow.com/questions/5852863/fixed-size-queue-which-automatically-dequeues-old-values-upon-new-enques
    // 抄代码真爽哦
    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        private readonly object syncObject = new object();

        public int Size { get; private set; }

        public FixedSizedQueue(int size)
        {
            Size = size;
        }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            lock (syncObject)
            {
                while (base.Count > Size)
                {
                    T outObj;
                    base.TryDequeue(out outObj);
                }
            }
        }
    }
}
