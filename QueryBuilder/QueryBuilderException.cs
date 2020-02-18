using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
