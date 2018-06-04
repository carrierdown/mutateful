using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Dto
{
    public class ProcessResultArray<T>
    {
        public bool Success { get; }
        public T[] Result { get; }
        public string ErrorMessage { get; }

        public ProcessResultArray(bool success, T[] result, string errorMessage)
        {
            Success = success;
            Result = result;
            ErrorMessage = errorMessage;
        }

        public ProcessResultArray(string errorMessage) : this(false, new T[0], errorMessage) { }

        public ProcessResultArray(T[] result) : this(true, result, "") { }
    }
}
