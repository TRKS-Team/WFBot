using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using GammaLibrary.Extensions;
using Humanizer;
using WFBot.Features.Utils;
using WFBot.Utils;
using Timer = System.Timers.Timer;

namespace WFBot.MahuaEvents
{
    public class InitEvent1
    {
        public static WebSocketHandler websocket;

        public InitEvent1()
        {
            if (Config.Instance.IsPublicBot)
            {
                websocket = new WebSocketHandler();
            }
        }
        /*
                static void StartServer()
                {

                    if (Config.Instance.IsPublicBot)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            var server = new NamedPipeServerStream("WFBot878527767");
                            server.WaitForConnection();
                            StreamReader reader = new StreamReader(server);
                            StreamWriter writer = new StreamWriter(server);
                            while (true)
                            {
                                var line = reader.ReadLine();
                                if (line.Contains("Update"))
                                {
                                    string result;
                                    using (var robotSession = MahuaRobotManager.Instance.CreateSession())
                                    {
                                        var mahuaApi = robotSession.MahuaApi;
                                        var members = mahuaApi.GetGroupMemebersWithModel("878527767").Model.ToList();
                                        result = string.Join(" ", members.Select(g => g.Qq));
                                    }
                                    writer.WriteLine(result);
                                    writer.Flush();
                                }
                            }
                        });
                    }
                }
                */

    }
}
