using Microsoft.Extensions.Hosting;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BackgroundPinger
{

    public class PingerService : BackgroundService
    {    
        private readonly Ping Pinger;
        private readonly ILogger Logger;
        private readonly IPingSettings PingSettings;
        public PingerService(ILogger logger, IPingSettings pingSettings)
        {
            PingSettings = pingSettings;
            Pinger = new Ping();
            Logger = logger;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(PingSettings.Frequency, stoppingToken);

                try
                {
                    var pingTask = Pinger.SendPingAsync(PingSettings.Target, (int)PingSettings.Timeout.TotalMilliseconds);
                    var cancelTask = Task.Delay(PingSettings.Timeout, stoppingToken);

                    //double await so exceptions from either task will bubble up
                    await await Task.WhenAny(pingTask, cancelTask);

                    if(pingTask.IsCompletedSuccessfully)
                    {
                        LogPingReply(pingTask.Result);
                    }
                    else
                    {
                        LogError("Ping didn't complete successfully");
                    }

                }
                catch(Exception ex)
                {
                    LogError(ex.Message);
                }
            }
        }
        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            if(Pinger != null)
            {
                Pinger.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }
        private void LogPingReply(PingReply pingReply)
        {
            Logger.Information($"PingReply status={pingReply.Status} roundTripTime={pingReply.RoundtripTime}");
        }
        private void LogError(string error)
        {
            Logger.Error(error);
        }
    }
}
