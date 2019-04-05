namespace Mutate4l.Core
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

        public Result(string errorMessage, string header) : this(false, $"{header}: {errorMessage}") { }
    }
}
