namespace Mutate4l.Core
{
    public class ProcessResult<T> where T:new()
    {
        public bool Success { get; }
        public T Result { get; }
        public string ErrorMessage { get; }

        public ProcessResult(bool success, T result, string errorMessage)
        {
            Success = success;
            Result = result;
            ErrorMessage = errorMessage;
        }

        public ProcessResult(string errorMessage) : this(false, new T(), $"Error: {errorMessage}") { }

        public ProcessResult(string errorMessage, string header) : this(false, new T(), $"{header}: {errorMessage}") { }

        public ProcessResult(T result) : this(true, result, "") { }
    }
}
