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
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
            {
                WebRootPath = "wwwroot"
            });
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            var serverPort = GetServerPort();

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
            builder.Logging.AddProvider(new WFBotLoggingProvider());
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

        public static int GetServerPort()
        {
            var serverPort = 9331;
            var port = Environment.GetEnvironmentVariable("WFBOT_WEBUI_PORT");
            if (!string.IsNullOrWhiteSpace(port) && ushort.TryParse(port, out var portNum))
            {
                serverPort = portNum;
            }

            return serverPort;
        }

        public static void NotifyDataChanged()
        {
            try
            {
                DataChanged?.Invoke(null, null);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"WebUI 通知UI更新出错: {e}");
            }
        }
    }

    public class WFBotLoggingProvider : ILoggerProvider
    {
        
        public ILogger CreateLogger(string categoryName)
        {
            return new WFBotWebUILogger(categoryName);
        }

        public void Dispose()
        {
        }
    }

    public class WFBotWebUILogger : ILogger
    {
        string _categoryName;

        public WFBotWebUILogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        class What : IDisposable
        {
            public void Dispose()
            {
            }
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return new What();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel is LogLevel.Error or LogLevel.Critical;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine($"WebUI [{logLevel}]: {formatter(state, exception)} {exception}");
        }
    }
}
