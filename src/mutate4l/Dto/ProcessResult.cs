using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Dto
{
    public class ProcessResult
    {
        public bool Success { get; }
        public Clip[] Result { get; }
        public string ErrorMessage { get; }

        public ProcessResult(bool success, Clip[] result, string errorMessage)
        {
            Success = success;
            Result = result;
            ErrorMessage = errorMessage;
        }

        public ProcessResult(string errorMessage) : this(false, null, errorMessage) { }

        public ProcessResult(Clip[] result) : this(true, result, "") { }
    }
}
