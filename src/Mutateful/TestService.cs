using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Mutateful.Hubs;

namespace Mutateful
{
    public class TestService : IHostedService
    {
        private Timer Timer;
        private readonly IHubContext<MutatefulHub> MutatefulHub;
        
        public TestService(IHubContext<MutatefulHub> mutatefulHub)
        {
            MutatefulHub = mutatefulHub;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Timer = new Timer(SendTestMessage, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void SendTestMessage(object state)
        {
            Console.WriteLine("Sending test message...");
            MutatefulHub.Clients.All.SendAsync("DebugMessage", "A1", 10);
        }

        public void Dispose()
        {
            Timer?.Dispose();
        }
    }
}