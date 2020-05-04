using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DBApi
{
    /// <summary>
    /// Describes a Database Statement
    /// </summary>
    public interface IStatement : IDisposable
    {
        /// <summary>
        /// Binds a parameter pair in the statement
        /// </summary>
        /// <param name="name">The Name of the Parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>The object itself</returns>
        IStatement BindParameter(string name, object value);
        /// <summary>
        /// Binds a parameter in the statement
        /// </summary>
        /// <param name="sqlParameter">The SqlParameter to be bound</param>
        /// <returns>The object itself</returns>
        IStatement BindParameter(SqlParameter sqlParameter);
        /// <summary>
        /// Binds all parameters at once
        /// </summary>
        /// <param name="Parameters">A dictionary containing the pair of the parameters</param>
        /// <returns>the object itself</returns>
        IStatement BindParameters(Dictionary<string, object> Parameters);
        /// <summary>
        /// Sets or unsets the transaction object of the statement
        /// </summary>
        /// <param name="sqlTransaction">The Transaction object</param>
        /// <returns>the object itself</returns>
        IStatement SetTransaction(SqlTransaction? sqlTransaction = null);
        /// <summary>
        /// Executes a Query
        /// </summary>
        /// <returns>The number of rows affected by the query</returns>
        int Execute();
        /// <summary>
        /// Fetches data from the database
        /// </summary>
        /// <returns>A data table with all the results or null, if the query did not produce results</returns>
        DataTable Fetch();
        /// <summary>
        /// Fetches data from the database
        /// </summary>
        /// <param name="RowNumber">The row number to be fetched, if more than one rows exist in the datase</param>
        /// <returns>A single row or null if the query did not produce results</returns>
        DataRow FetchRow(int RowNumber = 0);
        /// <summary>
        /// Fetches data from the database
        /// </summary>
        /// <param name="ColumnNumber">The Column number to be fetched, if more that one columns exists in the row</param>
        /// <param name="RowNumber">The row number to be fetched, if more than one rows exist in the datases</param>
        /// <returns>The value or null if the query did not produce results</returns>
        object FetchValue(int ColumnNumber = 0, int RowNumber = 0);
        /// <summary>
        /// Fetches data from database
        /// </summary>
        /// <returns>The value or null if the query did not produce results</returns>
        object FetchScalar();
        /// <summary>
        /// Checks if the statement is "dirty" - i.e. it has already run.
        /// </summary>
        /// <returns>True if the statement is dirty, otherwise false</returns>
        bool IsDirty();
    }
}
