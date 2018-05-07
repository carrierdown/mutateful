using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Dto
{
    public class Result
    {
        public bool Success { get; }
        public string ErrorMessage { get; }

        public Result(bool success, string errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }

        public Result(string errorMessage) : this(false, $"Error: {errorMessage}") { }
    }
}
