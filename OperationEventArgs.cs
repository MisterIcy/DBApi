using System;

namespace DBApi
{
    public class OperationEventArgs : EventArgs
    {
        public string OperationName { get; }
        public bool IsSuccess { get; }
        public long ElapsedMillis { get; }
        public string Caller { get; }
        public Exception Exception { get; }

        public OperationEventArgs (
            string OperationName, bool IsSuccess = true,
            long ElapsedMillis = 0, string Caller = null,
            Exception exception = null)
        {
            if (string.IsNullOrEmpty(OperationName))
                throw new ArgumentNullException(nameof(OperationName));
            this.OperationName = OperationName;
            this.IsSuccess = IsSuccess;
            this.ElapsedMillis = ElapsedMillis;
            this.Caller = Caller;
            this.Exception = exception;
        }
    }
}
