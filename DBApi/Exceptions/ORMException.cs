using System;
using System.Runtime.Serialization;

namespace DBApi.Exceptions
{
    public class ORMException : Exception
    {
        #region Default Constructors
        public ORMException()
        {
        }

        public ORMException(string message) : base(message)
        {
        }

        public ORMException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ORMException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        #endregion

    }
    public class ORMStatementException : ORMException
    {
        public string Sql { get; }
        public ORMStatementException(string Sql, string message) : base(message)
        {
            this.Sql = Sql;
        }
        public static ORMStatementException DirtyStatement(string Sql)
        {
            return new ORMStatementException(Sql, "This statement is `dirty` and cannot be executed again");
        }
    }
}
