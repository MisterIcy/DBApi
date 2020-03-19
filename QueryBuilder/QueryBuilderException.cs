using System;

namespace DBApi.QueryBuilder
{
    internal class QueryBuilderException : Exception
    {
        public QueryBuilderException(string message)
            : base(message) { }

        internal static QueryBuilderException QueryAlreadyDefined()
        {
            return new QueryBuilderException("This query is already defined");
        } 
    }
}
