namespace Mutateful.Hubs;

public interface IMutatefulHub
{
    Task SetClipDataOnClient(bool isLive11, byte[] data);
    Task SetClipDataOnWebUI(string clipRef, byte[] data);
    Task SetFormulaOnWebUI(int trackNo, int clipNo, string formula);
}