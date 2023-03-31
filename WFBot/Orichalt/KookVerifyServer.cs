using Microsoft.AspNetCore.SignalR.Client;
using KookConfig = WFBot.Orichalt.OrichaltConnectors.KookConfig;

namespace WFBot.Orichalt
{
    public class KookVerifyServer
    {
        private HubConnection _connection =
            new HubConnectionBuilder()
                .WithUrl(KookConfig.Instance.VerifyServerUrl)
                .Build();

        private bool connected;
        public void Init()
        {
            try
            {
                _connection.StartAsync();
                connected = false;
            }
            catch (Exception e)
            {

            }
            _connection.Closed += async (error) =>
            {
                connected = false;
                while (true)
                {
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    try
                    {
                        await _connection.StartAsync();
                        connected = true;
                        break;
                    }
                    catch (Exception e)
                    {
                        MiguelNetwork.SendDebugInfo($"Kook验证服务器连接出错: {e}");
                    }
                }
            };
        }

        private ulong GetGuildID(OrichaltContext o)
        {
            var kookContext = MiguelNetwork.OrichaltContextManager.GetKookContext(o);
            return kookContext.Guild.Id;
        }
        public async Task<bool> RedeemCode(OrichaltContext o, string code)
        {
            return await _connection.InvokeAsync<bool>("RedeemCode", GetGuildID(o), code);
        }

        public async Task<bool> VerifyGuild(OrichaltContext o)
        {
            return await _connection.InvokeAsync<bool>("VerifyGuild", GetGuildID(o));
        }

        public async Task<DateTime> GetGuildValidationTime(OrichaltContext o)
        {
            return await _connection.InvokeAsync<DateTime>("GetGuildValidationTime", GetGuildID(o));
        }
        public async Task<bool> StartGuildTrial(OrichaltContext o)
        {
            return await _connection.InvokeAsync<bool>("StartGuildTrial", GetGuildID(o));
        }
    }
}
