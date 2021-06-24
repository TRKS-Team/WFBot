using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GammaLibrary.Extensions;
using TextCommandCore;
using WFBot.Features.Commands;
using WFBot.Features.Events;
using WFBot.Utils;

namespace WFBot.Features.Other
{
    public class CustomCommandMatcherHandler : ICommandHandler<CustomCommandMatcherHandler>
    {
        static Lazy<CommandInfo[]> _commandInfos =
            new Lazy<CommandInfo[]>(CommandHandlerHelper.GetCommandInfos<CommandsHandler>());

        static Dictionary<string, List<string>> CustomCommandsRegistry => CustomCommandConfig.Instance.CustomCommands;

        static volatile bool _inited;

        public static void InitCustomCommandHandler()
        {
            if (_inited) return;
            _inited = true;

            CustomCommandConfig.Update();
            foreach (var info in _commandInfos.Value)
            {
                info.Matchers.Add(s =>
                {
                    var commandID = info.Method.Name;
                    if (ContainsCustomCommand(commandID))
                    {
                        return CustomCommandConfig.Instance.CustomCommands[commandID]
                            .Any(customMatcher => customMatcher == s);
                    }
                    return false;
                });
            }
        }

        [MatchersIgnoreCase("PrintCustomCommandMatchers")]
        void PrintCustomCommands()
        {
            bool found = false;
            foreach (var info in _commandInfos.Value)
            {
                var commandID = info.Method.Name;
                Console.WriteLine($"ID: {commandID}");
                if (ContainsCustomCommand(commandID))
                {
                    Console.WriteLine($"    自定义匹配器: [{GetCustomCommandList(commandID).Connect()}]\n");
                    found = true;
                }
            }

            if (!found)
            {
                Console.WriteLine("没有一个命令有自定义匹配器.");
            }
        }

        [MatchersIgnoreCase("PrintCommands")]
        void PrintCommands()
        {
            foreach (var info in _commandInfos.Value)
            {
                var commandID = info.Method.Name;
                Console.WriteLine($"ID: {commandID}\n" +
                                  $"    预定义匹配器: [{info.Method.GetCustomAttribute<MatchersAttribute>().Matchers.Connect()}]");
                if (ContainsCustomCommand(commandID))
                {
                    Console.WriteLine($"    自定义匹配器: [{GetCustomCommandList(commandID).Connect()}]\n");
                }
            }
        }

        static bool ContainsCustomCommand(string commandID)
        {
            return CustomCommandsRegistry.ContainsKey(commandID);
        }

        static List<string> GetCustomCommandList(string commandID)
        {
            if (!ContainsCustomCommand(commandID)) CustomCommandsRegistry[commandID] = new List<string>();
            return CustomCommandsRegistry[commandID];
        }

        [MatchersIgnoreCase("AddCustomCommandMatcher")]
        string AddCustomCommandMatcher(string commandID, string matcher)
        {
            if (_commandInfos.Value.All(info => info.Method.Name != commandID))
            {
                return "找不到这个命令 ID.";
            }
            var list = GetCustomCommandList(commandID);
            if (list.Contains(matcher)) return "请不要重复添加.";
            
            list.Add(matcher);
            SaveConfig();
            return "添加完成.";
        }

        [MatchersIgnoreCase("RemoveCustomCommandMatcher")]
        string RemoveCustomCommandMatcher(string commandID, string matcher)
        {
            if (_commandInfos.Value.All(info => info.Method.Name != commandID))
            {
                return "找不到这样的命令 ID.";
            }
            var list = GetCustomCommandList(commandID);
            if (!list.Contains(matcher))
            {
                return "找不到这样的 Matcher.";
            }

            list.Remove(matcher);
            if (list.IsEmpty()) CustomCommandsRegistry.Remove(commandID);
            SaveConfig();
            return "移除完成.";
        }

        void SaveConfig() => CustomCommandConfig.Save();

        public Action<TargetID, Message> MessageSender { get; } = (id, s) => Console.WriteLine(s);
        public Action<Message> ErrorMessageSender { get; } = s => Console.WriteLine(s);
        public string Sender { get; } = "";
        public string Message { get; }

        public CustomCommandMatcherHandler(string message)
        {
            Message = message;
        }
    }

    [Configuration("CustomCommands")]
    public class CustomCommandConfig : Configuration<CustomCommandConfig>
    {
        public Dictionary<string, List<string>> CustomCommands { get; set; } = new Dictionary<string, List<string>>();
    }
}
