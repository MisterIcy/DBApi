using System;
using System.Collections.Generic;
using System.Text;

namespace DBApi
{
    public class OperationEventArgs : EventArgs
    {
        public string OperationName { get; private set; }
        public bool IsSuccess { get; private set; }
        public long ElapsedMillis { get; private set; }
        public string Caller { get; private set; }
        public Exception Exception { get; private set; }

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
