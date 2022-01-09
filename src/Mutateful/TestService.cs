namespace Mutateful;

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
        /*Console.WriteLine("Sending test message...");
        MutatefulHub.Clients.All.SendAsync("SetClipDataOnClient", "A1", new byte[]
        {
            0,0,0,0,128,64,1,4,0,36,0,0,0,0,0,0,128,62,0,0,200,66,0,0,128,63,0,0,0,0,0,0,128,66,38,0,0,0,63,154,153,25,62,0,0,200,66,0,0,128,63,0,0,0,0,0,0,128,66,40,0,0,128,63,154,153,153,62,0,0,200,66,0,0,128,63,0,0,0,0,0,0,128,66,42,0,0,0,64,0,0,0,64,0,0,180,66,0,0,128,63,0,0,0,0,0,0,128,66
        });*/
    }

    public void Dispose()
    {
        Timer?.Dispose();
    }
}