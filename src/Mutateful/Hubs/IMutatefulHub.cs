using System.Threading.Tasks;

namespace Mutateful.Hubs
{
    public interface IMutatefulHub
    {
        Task FromServerMessage(string message);
        Task FromServerByteArray(byte[] data);
        Task SetClipDataOnClient(bool isLive11, byte[] data);
        
    }
}