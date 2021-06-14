namespace Mutateful.Core
{
    public class ProcessResult<T> where T:new()
    {
        public bool Success { get; }
        public T Result { get; }
        public string ErrorMessage { get; }

        public string WarningMessage { get; }

        public ProcessResult(bool success, T result, string errorMessage, string warningMessage = "")
        {
            Success = success;
            Result = result;
            ErrorMessage = errorMessage;
            WarningMessage = warningMessage;
        }

        public ProcessResult(string errorMessage) : this(false, new T(), $"Error: {errorMessage}") { }

        public ProcessResult(string errorMessage, string header) : this(false, new T(), $"{header}: {errorMessage}") { }

        public ProcessResult(T result) : this(true, result, "") { }
        
        public ProcessResult(T result, string warningMessage) : this(true, result, "", warningMessage) { }
        
        public void Deconstruct(out bool success, out T result, out string errorMessage)
        {
            success = Success;
            result = Result;
            errorMessage = ErrorMessage;
        }
    }
}
