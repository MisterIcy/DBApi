using System;
using System.Runtime.Serialization;
using DBApi.Annotations;

namespace DBApi.Exceptions
{
    public class OrmException : Exception
    {
        #region Default Constructors
        public OrmException()
        {
        }

        public OrmException(string message) : base(message)
        {
        }

        public OrmException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OrmException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        #endregion

    }
    public class OrmStatementException : OrmException
    {
        [PublicAPI]
        public string Sql { get; }
        [PublicAPI]
        public OrmStatementException(string Sql, string message) : base(message)
        {
            this.Sql = Sql;
        }
        [PublicAPI]
        public static OrmStatementException DirtyStatement(string Sql)
        {
            return new OrmStatementException(Sql, "This statement is `dirty` and cannot be executed again");
        }
    }
}
