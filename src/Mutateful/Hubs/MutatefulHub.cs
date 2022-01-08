using Decoder = Mutateful.IO.Decoder;

namespace Mutateful.Hubs;

public class MutatefulHub : Hub<IMutatefulHub>
{
    private readonly CommandHandler CommandHandler;

    public MutatefulHub(CommandHandler commandHandler)
    {
        CommandHandler = commandHandler;
    }

    public Task SetClipData(bool isLive11, byte[] data)
    {
        var clip = isLive11 ? Decoder.GetSingleLive11Clip(data) : Decoder.GetSingleClip(data);
        Console.WriteLine($"{clip.ClipReference.Track}, {clip.ClipReference.Clip} Incoming clip data");
        CommandHandler.SetClipData(clip);
        return Task.CompletedTask;
    }

    public Task SetFormula(byte[] data)
    {
        var (trackNo, clipNo, formula) = Decoder.GetFormula(data);
        Console.WriteLine($"{trackNo}, {clipNo}: Incoming formula {formula}");
        CommandHandler.SetFormula(trackNo, clipNo, formula);
        return Task.CompletedTask;
    }

    public async Task SetAndEvaluateClipData(bool isLive11, byte[] data)
    {
        var clip = isLive11 ? Decoder.GetSingleLive11Clip(data) : Decoder.GetSingleClip(data);
        Console.WriteLine($"{clip.ClipReference.Track}, {clip.ClipReference.Clip} Incoming clip data to evaluate");
        var result = CommandHandler.SetAndEvaluateClipData(clip);
        if (result.RanToCompletion == false) return;

        PrintErrors(result.Errors);

        foreach (var successfulClip in result.SuccessfulClips)
        {
            await Clients.All.SetClipDataOnClient(isLive11,
                isLive11
                    ? IOUtilities.GetClipAsBytesLive11(successfulClip).ToArray()
                    : IOUtilities.GetClipAsBytesV2(successfulClip).ToArray());
        }
    }

    public async Task SetAndEvaluateFormula(bool isLive11, byte[] data)
    {
        var (trackNo, clipNo, formula) = Decoder.GetFormula(data);
        Console.WriteLine($"{trackNo}, {clipNo}: Incoming formula {formula}");
        var result = CommandHandler.SetAndEvaluateFormula(formula, trackNo, clipNo);
        if (result.RanToCompletion == false) return;

        PrintErrors(result.Errors);

        foreach (var clip in result.SuccessfulClips)
        {
            await Clients.All.SetClipDataOnClient(isLive11,
                isLive11
                    ? IOUtilities.GetClipAsBytesLive11(clip).ToArray()
                    : IOUtilities.GetClipAsBytesV2(clip).ToArray());
        }
    }

    public async Task EvaluateFormulas(bool isLive11)
    {
        var result = CommandHandler.EvaluateFormulas();
        PrintErrors(result.Errors);

        foreach (var clip in result.SuccessfulClips)
        {
            await Clients.All.SetClipDataOnClient(isLive11,
                isLive11
                    ? IOUtilities.GetClipAsBytesLive11(clip).ToArray()
                    : IOUtilities.GetClipAsBytesV2(clip).ToArray());
        }
    }

    public Task LogMessage(byte[] data)
    {
        var text = Decoder.GetText(data);
        Console.WriteLine($"From client: {text}");
        return Task.CompletedTask;
    }

    private void PrintErrors(List<string> errorMessages)
    {
        foreach (var errorMessage in errorMessages)
        {
            Console.WriteLine(errorMessage);
        }
    }
}