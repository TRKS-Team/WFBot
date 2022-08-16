using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;

namespace WFBot.WebUI
{
    public class WFBotWebUIServer
    {
        public static event EventHandler DataChanged;

        public void Run()
        {
            var builder = WebApplication.CreateBuilder();
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            var serverPort = 9331;
            var port = Environment.GetEnvironmentVariable("WFBOT_WEBUI_PORT");
            if (!string.IsNullOrWhiteSpace(port) && ushort.TryParse(port, out var portNum))
            {
                serverPort = portNum;
            }
            builder.WebHost.ConfigureKestrel((context, serverOptions) =>
            {
                serverOptions.Listen(IPAddress.Any, serverPort);
            });
            
            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.Configure<RazorPagesOptions>(options => options.RootDirectory = "/WebUI/Pages");
            builder.WebHost.UseStaticWebAssets();
            builder.Logging.ClearProviders();
            var app = builder.Build();
            
            app.UseExceptionHandler("/Error");

            
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new ManifestEmbeddedFileProvider(
                    typeof(Program).Assembly, "wwwroot"
                )
            });
            app.UseStaticFiles();
            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.RunAsync();

        }

        public static void NotifyDataChanged()
        {
            DataChanged?.Invoke(null, null);
        }
    }
}
