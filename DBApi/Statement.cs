using DBApi.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DBApi
{
    public class Statement : IStatement
    {
        public string Sql { get; }
        private bool dirty = false;

        private readonly SqlCommand command;
        public Statement(string Query, SqlConnection connection)
        {
            if (string.IsNullOrEmpty(Query))
                throw new ArgumentNullException(nameof(Query));

            this.Sql = Query;
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            this.command = new SqlCommand(Sql, connection);
            this.command.Prepare();
        }
        /// <inheritdoc/>
        public IStatement BindParameter(string name, object value)
        {
            if (value == null)
                value = DBNull.Value;
            if (value is DateTime dateTime && dateTime == DateTime.MinValue)
                value = DBNull.Value;

            this.command.Parameters.Add(new SqlParameter(name, value));
            return this;
        }

        public IStatement BindParameter(SqlParameter sqlParameter)
        {
            this.command.Parameters.Add(sqlParameter);
            return this;
        }

        public IStatement BindParameters(Dictionary<string, object> Parameters)
        {
            if (Parameters != null && Parameters.Count > 0)
            {
                foreach (var Parameter in Parameters)
                {
                    BindParameter(Parameter.Key, Parameter.Value);
                }
            }
            return this;
        }

        public IStatement SetTransaction(SqlTransaction? sqlTransaction = null)
        {
            this.command.Transaction = sqlTransaction;
            return this;
        }

        public int Execute()
        {
            if (IsDirty())
                throw ORMStatementException.DirtyStatement(this.Sql);

            this.dirty = true;
            return this.command.ExecuteNonQuery();
        }

        public DataTable Fetch()
        {
            if (IsDirty())
                throw ORMStatementException.DirtyStatement(this.Sql);

            DataTable table = new DataTable();
            using (SqlDataAdapter adapter = new SqlDataAdapter(this.command))
            {
                adapter.Fill(table);
            }
            this.dirty = true;
            return table;
        }

        public DataRow FetchRow(int RowNumber = 0)
        {
            if (IsDirty())
                throw ORMStatementException.DirtyStatement(this.Sql);

            DataRow row = null;
            using (DataTable table = Fetch())
            {
                if (table != null && table.Rows.Count > RowNumber)
                    row = table.Rows[RowNumber];
            }
            this.dirty = true;
            return row;

        }

        public object FetchValue(int ColumnNumber = 0, int RowNumber = 0)
        {
            if (IsDirty())
                throw ORMStatementException.DirtyStatement(this.Sql);

            object value = null;
            DataRow row = FetchRow(RowNumber);
            if (row != null && row.ItemArray.Length > ColumnNumber)
                value = row[ColumnNumber];
            this.dirty = true;
            return value;
        }
        public object FetchScalar()
        {
            if (IsDirty())
                throw ORMStatementException.DirtyStatement(this.Sql);

            this.dirty = true;
            return this.command.ExecuteScalar();
        }

        public bool IsDirty()
        {
            return dirty;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.command.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Statement()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
