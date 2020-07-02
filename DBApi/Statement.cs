using DBApi.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DBApi.Annotations;

namespace DBApi
{
    public sealed class Statement : IStatement
    {
        /// <summary>
        /// The SQL Query to be performed on database
        /// </summary>
        [PublicAPI]
        public string Sql { get; }
        
        private bool _dirty;

        private int _commandTimeout;
        /// <summary>
        /// Sets or gets a value that indicates the timeout of the command
        /// </summary>
        [PublicAPI]
        public int CommandTimeout
        {
            get => _commandTimeout;
            set
            {
                if (IsDirty()) return;
                _commandTimeout = value;
                _command.CommandTimeout = _commandTimeout;
            }
        }
        
        private readonly SqlCommand _command;
        public Statement(string query, SqlConnection connection, int commandTimeout = 30)
        {
            CommandTimeout = commandTimeout;
            if (string.IsNullOrEmpty(query))
                throw new ArgumentNullException(nameof(query));

            Sql = query;
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            _command = new SqlCommand(Sql, connection) {CommandTimeout = CommandTimeout};
            _command.Prepare();
        }
        /// <inheritdoc/>
        public IStatement BindParameter(string name, object value)
        {
            value ??= DBNull.Value;
            
            if (value is DateTime dateTime && dateTime == DateTime.MinValue)
                value = DBNull.Value;

            _command.Parameters.Add(new SqlParameter(name, value));
            return this;
        }

        public IStatement BindParameter(SqlParameter sqlParameter)
        {
            _command.Parameters.Add(sqlParameter);
            return this;
        }

        public IStatement BindParameters(Dictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count <= 0) return this;
            
            foreach (var parameter in parameters)
            {
                BindParameter(parameter.Key, parameter.Value);
            }
            return this;
        }

        public IStatement SetTransaction(SqlTransaction? sqlTransaction = null)
        {
            _command.Transaction = sqlTransaction;
            return this;
        }

        public int Execute()
        {
            if (IsDirty())
                throw ORMStatementException.DirtyStatement(Sql);

            _dirty = true;
            return _command.ExecuteNonQuery();
        }

        public DataTable Fetch()
        {
            if (IsDirty())
                throw ORMStatementException.DirtyStatement(Sql);

            DataTable table = new DataTable();
            using (SqlDataAdapter adapter = new SqlDataAdapter(_command))
            {
                adapter.Fill(table);
            }
            _dirty = true;
            return table;
        }

        public DataRow? FetchRow(int rowNumber = 0)
        {
            if (IsDirty())
                throw ORMStatementException.DirtyStatement(Sql);

            DataRow? row = null;
            using (DataTable table = Fetch())
            {
                if (table != null && table.Rows.Count > rowNumber)
                    row = table.Rows[rowNumber];
            }
            _dirty = true;
            return row;

        }

        public object? FetchValue(int columnNumber = 0, int rowNumber = 0)
        {
            if (IsDirty())
                throw ORMStatementException.DirtyStatement(Sql);

            object? value = null;
            DataRow? row = FetchRow(rowNumber);
            if (row != null && row.ItemArray.Length > columnNumber)
                value = row[columnNumber];
            _dirty = true;
            return value;
        }
        public object FetchScalar()
        {
            if (IsDirty())
                throw ORMStatementException.DirtyStatement(this.Sql);

            _dirty = true;
            return _command.ExecuteScalar();
        }

        public bool IsDirty()
        {
            return _dirty;
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this._command.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
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
