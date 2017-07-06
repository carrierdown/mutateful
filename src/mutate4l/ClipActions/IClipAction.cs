using System;
using System.Collections.Generic;
using System.Text;
using Mutate4l.Cli;
using Mutate4l.Dto;
using Mutate4l.IO;

namespace Mutate4l.ClipActions
{
    interface IClipAction
    {
        Clip Apply(Clip a, Clip b);
    }
}
