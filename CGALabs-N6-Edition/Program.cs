using CGALabs_N6_Edition.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CGALabs_N6_Edition
{
    internal static class Program
    {
        private static IHost _host;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            _host = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<Form1>();
                    services.AddScoped<IObjectFileReader, ObjectFileReader>();
                }).ConfigureLogging(logBuilder =>
                {
                    logBuilder.SetMinimumLevel(LogLevel.Information);
                    logBuilder.AddNLog("nlog.config");
                }).Build();

            using var serviceScope = _host.Services.CreateScope();
            {
                var services = serviceScope.ServiceProvider;
                try
                {
                    var form = services.GetRequiredService<Form1>();

                    Application.Run(form);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error occured: {e.Message}");
                }
            }
        }
    }
}