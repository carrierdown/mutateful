namespace Mutateful.Hubs;

public interface IMutatefulHub
{
    Task SetClipDataOnClient(bool isLive11, byte[] data);
}