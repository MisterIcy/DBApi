using DBApi.Reflection;
using System;
using System.Collections.Generic;
using DBApi.QueryBuilder;
using System.Text;

namespace DBApi
{
    public interface IQuery
    {
        /// <summary>
        /// Returns the Sql Server Query
        /// </summary>
        /// <returns></returns>
        string GetNativeQuery();

        IQuery Select(ClassMetadata metadata);
        IQuery Select(params string[] fields);
        IQuery Select(string expression);
        IQuery From(ClassMetadata metadata, string alias = null);
        IQuery From(string tableName, string alias = null);
        IQuery From(IQuery internalQuery, string alias = null);
        IQuery Where(Expression expression);
        IQuery AndWhere(Expression expression);
        IQuery OrWhere(Expression expression);
        IQuery OrderBy(string expression);
        IQuery OrderBy(string field, string order);
        IQuery Join(string tableName, string tableAlias, Expression on);
        IQuery InnerJoin(string tableName, string tableAlias, Expression on);
        IQuery LeftJoin(string tableName, string tableAlias, Expression on);
        IQuery RightJoin(string tableName, string tableAlias, Expression on);
        IQuery GroupBy(string field);
        IQuery Having(Expression expression);
        IQuery Like(string field, object term);
        IQuery Top(int rows, bool percentage = false);
        IQuery Insert(ClassMetadata metadata);
        IQuery Insert(string tableName, params string[] fields);
        IQuery Update(ClassMetadata metadata);
        IQuery Update(string tableName, params string[] fields);
        IQuery Delete(ClassMetadata metadata);
        IQuery BeginTransaction();
        IQuery Commit();
        IQuery Rollback();

    }
}
