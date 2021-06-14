namespace Mutateful.Core
{
    public class ProcessResultArray<T>
    {
        public bool Success { get; }
        public T[] Result { get; }
        public string ErrorMessage { get; }
        
        public string WarningMessage { get; }

        public ProcessResultArray(bool success, T[] result, string errorMessage, string warningMessage = "")
        {
            Success = success;
            Result = result;
            ErrorMessage = errorMessage;
            WarningMessage = warningMessage;
        }

        public ProcessResultArray(string errorMessage) : this(false, new T[0], errorMessage) { }
        
        public ProcessResultArray(T[] result, string warningMessage) : this(true, result, "", warningMessage) { }

        public ProcessResultArray(T[] result) : this(true, result, "") { }

        public void Deconstruct(out bool success, out T[] result, out string errorMessage)
        {
            success = Success;
            result = Result;
            errorMessage = ErrorMessage;
        }

        public void Deconstruct(out bool success, out T[] result, out string errorMessage, out string warningMessage)
        {
            success = Success;
            result = Result;
            errorMessage = ErrorMessage;
            warningMessage = WarningMessage;
        }
    }
}
