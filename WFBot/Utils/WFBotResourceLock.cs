using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace WFBot.Utils
{

    public class WFBotResourceLock : IDisposable
    {
        static readonly HashSet<WFBotResourceLock> Locks = new HashSet<WFBotResourceLock>();
        public static ImmutableArray<WFBotResourceLock> AllLocks => Locks.ToImmutableArray();
        public static bool AnyLockAcquired => Locks.Any();

        public bool Locked { get; private set; }
        public string Name { get; }
        public ResourceLockTypes LockType { get; }
        public WFBotResourceLock(string name, ResourceLockTypes type = ResourceLockTypes.Whatever)
        {
            if (WFBotCore.IsShuttingDown)
                throw new InvalidOperationException(
                    "You cannot acquire a resource lock while the app is shutting down.");
            Name = name;
            LockType = type;

            lock (Locks)
            {
                Locked = true;
                Locks.Add(this);
            }
        }

        public static WFBotResourceLock Create(string name, ResourceLockTypes type = ResourceLockTypes.Whatever)
        {
            return new WFBotResourceLock(name, type);
        }

        public void Dispose()
        {
            lock (Locks)
            {
                Locked = false;
                if (Locks.Contains(this))
                {
                    Locks.Remove(this);
                }

                if (WFBotCore.IsShuttingDown)
                {
                    Console.WriteLine($"资源 {Name} 已经释放.");
                }
            }

        }
    }

    public enum ResourceLockTypes
    {
        Essential,
        Whatever
    }
}
