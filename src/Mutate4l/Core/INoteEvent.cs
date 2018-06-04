using System;
namespace Mutate4l.Core
{
    public interface INoteEvent
    {
        decimal Start { get; }
        decimal End { get; }
        decimal Duration { get; }
    }
}
