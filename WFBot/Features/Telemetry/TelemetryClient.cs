﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using GammaLibrary.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using WFBot.Orichalt;
using InternetTime;

namespace WFBot.Features.Telemetry
{

    public static class TelemetryClient
    {
        public static string ClientID { get; set; } = GetClientID();

        static HubConnection connection;

        static string GetClientID()
        {
            var mem = new MemoryMetricsClient().GetMetrics().Total;
            var cpu = Environment.ProcessorCount;
            var path = Environment.CurrentDirectory;
            var guid = Config.Instance.ClientGUID;
            return (mem + cpu + path + guid).SHA256().ToHexString().Substring(0, 6);
        }

        public static volatile bool connected;
        public static async Task Start()
        {
            if (!Config.Instance.UseTelemetry) return;
            
            Console.WriteLine("此版本 WFBot 带有数据收集来改进用户体验. 收集的内容包括基本系统信息、启动时间、命令处理异常、是否在线等, 并且已经匿名化处理. "
                              //"如果你不喜欢, 可以在 Config.json 中关闭."
                              );

            connection = new HubConnectionBuilder()
                .WithUrl("https://wfbot.cyan.cafe/WFBotClientHub")
                .Build();
            

            connection.Closed += async (error) =>
            {
                connected = false;
                while (true)
                {
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    try
                    {
                        await connection.StartAsync();
                        connected = true;
                        break;
                    }
                    catch (Exception e)
                    {
                    }
                }
            };
            
            connection.On<Guid>("Report", g =>
            {
                try
                {
                    var memory = new MemoryMetricsClient().GetMetrics();
                    string arch;
                    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
                    {
                        arch = "Docker";
                    }
                    else
                    {
                        if (OperatingSystem.IsLinux())
                        {
                            arch = "Linux";
                            try
                            {
                                var file = File.ReadAllText("/etc/lsb-release").Split('\n')
                                    .First(l => l.Contains("DISTRIB_DESCRIPTION")).Split('=')[1].Trim('\"');
                                arch = file;
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else if (OperatingSystem.IsWindows())
                        {
                            arch = Environment.OSVersion.ToString();
                        }
                        else
                        {
                            arch = "Unknown";
                        }
                    }

                    connection.InvokeCoreAsync("ReportServer", new object[]{g, new WFBotTelemetryReport()
                    {
#if DEBUG
                        Build = "DEBUG",
#else
                    Build = "RELEASE",
#endif
                        Version = WFBotCore.Version,
                        CPUCores = Environment.ProcessorCount.ToString(),
                        Memory = $"{memory.Used/1024.0:F1}GB/{memory.Total/1024.0:F1}GB",
                        ClientID = ClientID,
                        Connector = Config.Instance.Miguel_Platform.ToString(),
                        InstanceCommandsProcessed = WFBotCore.InstanceCommandsProcessed, //todo
                        InstanceMessagesProcessed = WFBotCore.InstanceMessagesProcessed,
                        StartupTime = WFBotCore.StartUpTime,
                        TimeDifferenceFromRealTime = WFBotCore.TimeDelayFromRealTime.TotalMinutes.ToString("F1") + "min",
                        Arch = arch,
                        WFBotStorageSize = (new DirectoryInfo(".").EnumerateFiles("*.*", SearchOption.AllDirectories).Select(f => f.Length).Sum() / 1024.0 / 1024.0).ToString("F1") + "MB"
                    }});
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            try
            {
                await connection.StartAsync();
                connected = true;   

            }
            catch (Exception e)
            {
                Console.WriteLine("Telemetry 启动失败.");
                Console.WriteLine(e);
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(1000);
                        try
                        {
                            await connection.StartAsync();
                            connected = true;
                            break;
                        }
                        catch (Exception exception)
                        {
                        }
                    }
                });
            }


        }

        public static void ReportCommand(CommandReport commandReport)
        {
            if (connected)
                connection.SendCoreAsync("ReportCommand", new Object[] { commandReport});

        }

        
        public static void ReportStarted(double startTime)
        {
            if (connected)
            {
#if !DEBUG
                connection.SendCoreAsync("ReportStarted", new object[] { new StartedReport(ClientID, startTime) });
#endif                
            }


        }
    }

    public record StartedReport(string ClientID, double StartUpTime);

    public class WFBotTelemetryReport
    {
        public string ClientID { get; set; }
        public string StartupTime { get; set; }
        public string Version { get; set; }
        public string CPUCores { set; get; }
        public string Memory { get; set; }
        public string Connector { get; set; }
        public string Build { get; set; }
        public int InstanceMessagesProcessed { get; set; }
        public int InstanceCommandsProcessed { get; set; }
        public string TimeDifferenceFromRealTime { get; set; }
        public string Arch { get; set; }
        public TimeSpan TimeDelay { get; set; }
        public string WFBotStorageSize { get; set; }

    }

    public record CommandReport(string GroupID, string UserID, string Command, string Result, string EndTime, string ProcessingTime, string ClientID);

    public static class TelemetryExtensions
    {
        public static string AnonymizeString(this string str)
        {
            var s = str + Config.Instance.ClientGUID;
            var bytes = s.SHA256();
            for (int i = 0; i < 100; i++)
            {
                bytes = bytes.SHA256();
            }

            return bytes.ToHexString().Substring(0, 8);
        }
    }
}
