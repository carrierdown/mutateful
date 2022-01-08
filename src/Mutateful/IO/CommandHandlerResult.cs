namespace Mutateful.IO;

public record CommandHandlerResult(bool RanToCompletion, List<Clip> SuccessfulClips, List<string> Errors, List<string> Warnings)
{
    public static readonly CommandHandlerResult CompletedResult =
        new CommandHandlerResult(true, new List<Clip>(), new List<string>(), new List<string>());
    
    public static readonly CommandHandlerResult AbortedResult = 
        new CommandHandlerResult(false, new List<Clip>(), new List<string>(), new List<string>());
};