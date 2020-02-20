using DBApi.QueryBuilder;
using DBApi.Reflection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using DBApi.Exceptions;
using System.Data;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;

namespace DBApi
{
    public class EntityManager : IEntityManager
    {
        public event EventHandler<OperationEventArgs> OperationComplete;
        protected virtual void OnOperationComplete(OperationEventArgs args)
        {
            OperationComplete?.Invoke(this, args);
        } 
        protected virtual void OnOperationComplete(string OperationName, bool IsSuccess = true, long ElapsedMillis = 0)
        {
            OnOperationComplete(new OperationEventArgs(OperationName, IsSuccess, ElapsedMillis));
        }
        /// <summary>
        /// Ο μέγιστος αριθμός προσπαθειών επανασύνδεσης σε περίπτωση προβλήματος επικοινωίας με τον SQL Server
        /// </summary>
        public static int MaxRetries { get; set; } = 5;

        private readonly string connectionString = string.Empty;

        public EntityManager(string ConnectionString)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new ArgumentNullException(nameof(ConnectionString));
            this.connectionString = ConnectionString;
        }

        public SqlConnection CreateSqlConnection() => new SqlConnection(this.connectionString);
        public QueryBuilder.QueryBuilder CreateQueryBuilder() => new QueryBuilder.QueryBuilder();
        private ClassMetadata GetClassMetadata(Type entityType)
        {
            return MetadataCache.Get(entityType);
        }
        private int GetLastInsertId(SqlConnection connection, SqlTransaction transaction = null)
        {
            if (connection == null || connection.State != System.Data.ConnectionState.Open)
            {
                throw new Exception("GetLastInsertId Requires a valid and open connection");
            }
            try
            {
                using (Statement stmt = new Statement("SELECT CONVERT(int, @@IDENTITY)", connection))
                {
                    if (transaction != null)
                        stmt.SetTransaction(transaction);

                    return (int)stmt.FetchScalar();
                }
            }
            catch (SqlException ex)
            {
                return -1;
            }
        }
        private int FastCount(string tableName, Dictionary<string, object> parameters)
        {
            var queryBuilder = CreateQueryBuilder()
                .Select("COUNT(*)")
                .From(tableName, "t");
            int paramNum = 0;
            if (parameters != null && parameters.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    if (paramNum == 0)
                        queryBuilder.Where(new Eq($"t.{parameter.Key}", $"@{parameter.Key}"));
                    else
                        queryBuilder.AndWhere(new Eq($"t.{parameter.Key}", $"@{parameter.Key}"));

                    paramNum++;
                }
            }
            string query = queryBuilder.GetQuery();
            using (SqlConnection Connection = CreateSqlConnection())
            {
                try
                {
                    Connection.Open();
                    using (Statement stmt = new Statement(query, Connection))
                    {
                        stmt.BindParameters(parameters);
                        return (int)stmt.FetchScalar();
                    }
                } catch (SqlException ex)
                {
                    //TODO: Handle errors
                    throw ex;
                }
            }
        }
        private int FastCountStar(ClassMetadata metadata, object identifier)
        {
            return FastCount(metadata.TableName, new Dictionary<string, object>()
            {
                {metadata.IdentifierColumn, identifier }
            });
        }


        public T Persist<T>(T entityObject) where T : class
        {
            return Persist(typeof(T), entityObject) as T;
        }
        private string GetOperationName(object obj = null, [CallerMemberName] string name = null)
        {
            return $"{name}:{obj.ToString()}";
        }
        public object Persist(Type entityType, object entityObject)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
#endif
            if (entityObject == null)
                throw new ArgumentNullException(nameof(entityObject));

            var metadata = GetClassMetadata(entityType);
            object identifier = metadata.GetColumnFieldInfo(metadata.IdentifierColumn)
                .GetValue(entityObject);

            if (identifier != null && ((Int32)identifier) != -1)
            {
                if (FastCountStar(metadata, identifier) > 0)
                    return Update(entityType, entityObject);
            }
#if DEBUG
            sw.Start();
#endif
            string Query = CreateQueryBuilder()
                .Insert(entityType)
                .GetQuery();
            int affectedRows = 0;
            int lastId = 0;
            SqlTransaction sqlTransaction = null;
            using (SqlConnection Connection = CreateSqlConnection())
            {
                try
                {
                    Connection.Open();
                    sqlTransaction = Connection.BeginTransaction();
                    using (Statement stmt = new Statement(Query, Connection))
                    {
                        affectedRows = stmt.SetTransaction(sqlTransaction)
                            .BindParameters(ClassMetadata.GetParameterDictionary(entityObject))
                            .Execute();
                    }
                    if (!metadata.HasGuidColumn())
                    {
                        lastId = GetLastInsertId(Connection, sqlTransaction);
                    } else
                    {
                        var gQuery = CreateQueryBuilder()
                            .Select(metadata.IdentifierColumn)
                            .From(metadata.TableName)
                            .Where(new Eq($"t.{metadata.GuidColumn}", "@guid"))
                            .GetQuery();

                        using (Statement stmt = new Statement(gQuery, Connection))
                        {
                            lastId = (int)stmt.SetTransaction(sqlTransaction)
                                .BindParameter("@guid", metadata.GetColumnFieldInfo(metadata.GuidColumn).GetValue(entityObject))
                                .FetchScalar();
                        }
                    }
                    metadata.GetColumnFieldInfo(metadata.IdentifierColumn)
                        .SetValue(entityObject, lastId);

                    if (metadata.HasCustomColumns())
                    {
                        var customColumns = metadata.Columns.Select(c => c.Value)
                           .Where(c => c.IsCustomColumn == true)
                           .ToList();
                        foreach (var customColumn in customColumns)
                        {
                            var cquery = customColumn.GetCustomColumnQuery();
                            var parm = customColumn.GetCustomColumnParameters(entityObject);
                            using (Statement stmt = new Statement(cquery, Connection))
                            {
                                stmt.SetTransaction(sqlTransaction)
                                    .BindParameters(parm)
                                    .Execute();
                            }
                        }
                    }
                    sqlTransaction.Commit();
                } catch (SqlException ex)
                {
                    if (sqlTransaction != null && Connection.State == System.Data.ConnectionState.Open)
                    {
                        sqlTransaction.Rollback();
                    }
                    Connection.Close();
                    throw new Exception(ex.Message, ex);
                    //throw new ORMStatementException(Query, ex.Message);
                }
            }
#if DEBUG
            sw.Stop();
            Debug.WriteLine($"Persisted ${entityObject} to database in ${sw.ElapsedMilliseconds} msec");
#endif
            entityObject = FindById(entityType, lastId);
            return entityObject;
        }

        public T Update<T>(T entityObject) where T : class
        {
            return Update(typeof(T), entityObject) as T;
        }

        public object Update(Type entityType, object entityObject)
        {
            if (entityObject == null) throw new ArgumentNullException(nameof(entityObject));

            ClassMetadata metadata = MetadataCache.Get(entityType);
            string Query = CreateQueryBuilder()
                .UpdateInternal(metadata)
                .Where(new Eq(metadata.IdentifierColumn, "@identifier"))
                .GetQuery();

            object identifier = metadata.GetIdentifierField().GetValue(entityObject);
            if (identifier == null || ((Int32)identifier) == -1)
                throw new ORMException("An object needs an identifier in order to be updated");

            SqlTransaction sqlTransaction = null;
            using (SqlConnection Connection = CreateSqlConnection())
            {
                try
                {
                    Connection.Open();
                    sqlTransaction = Connection.BeginTransaction();
                    using (Statement stmt = new Statement(Query, Connection))
                    {
                        stmt.SetTransaction(sqlTransaction)
                            .BindParameters(ClassMetadata.GetParameterDictionary(entityObject))
                            .BindParameter("@identifier", identifier)
                            .Execute();
                    }

                    if (metadata.HasCustomColumns())
                    {
                        var customColumns = metadata.Columns.Select(c => c.Value)
                           .Where(c => c.IsCustomColumn == true)
                           .ToList();
                        foreach (var customColumn in customColumns)
                        {
                            var cquery = customColumn.GetCustomColumnQuery();
                            var parm = customColumn.GetCustomColumnParameters(entityObject);
                            using (Statement stmt = new Statement(cquery, Connection))
                            {
                                stmt.SetTransaction(sqlTransaction)
                                    .BindParameters(parm)
                                    .Execute();
                            }
                        }
                    }
                    sqlTransaction.Commit();
                    
                } catch (SqlException ex)
                {
                    if (sqlTransaction != null && Connection.State == System.Data.ConnectionState.Open)
                        sqlTransaction.Rollback();
                    
                    throw new ORMStatementException(Query, ex.Message);
                }
                Connection.Close();
            }
            if (CacheManager.Contains(entityType, identifier))
            {
                CacheManager.Remove(entityType, identifier);
            }
            CacheManager.Add(entityType, identifier);
            //TODO: Check if we need to rehydrate
            return entityObject;
        }

        public T FindById<T>(object identifier) where T : class
        {
            return FindById(typeof(T), identifier) as T;
        }

        public object FindById(Type entityType, object identifier)
        {
            //Kill all Null Identifiers
            if (identifier == null)
                return null;
            if (((Int32)identifier) < 1)
                return null;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ClassMetadata metadata = GetClassMetadata(entityType);
            if (CacheManager.Contains(entityType, identifier))
            {
                sw.Stop();
                OnOperationComplete(GetOperationName($"/Fetched from Cache: {entityType.Name}:{identifier.ToString()}"), true, sw.ElapsedTicks);
                return CacheManager.Get(entityType, identifier);
            }

            sw.Restart();
            string Query = CreateQueryBuilder()
                .SelectInternal(metadata)
                .FromInternal(metadata)
                .Where(new Eq($"t.{metadata.IdentifierColumn}", "@identifier"))
                .GetQuery();
            sw.Stop();
            //OnOperationComplete(GetOperationName($"/Created Query: {entityType.Name}:{identifier.ToString()}"), true, sw.ElapsedTicks);

            object entity = null;
            sw.Restart();
            using (SqlConnection Connection = CreateSqlConnection())
            {
                Connection.Open();
                using (Statement stmt = new Statement(Query, Connection))
                {
                    stmt.BindParameter("@identifier", identifier);
                    entity = HydrateObject(stmt.FetchRow(), metadata);
                }
                Connection.Close();
            }
            sw.Stop();
            OnOperationComplete(GetOperationName($"/FindById Completed {entityType.Name}:{identifier.ToString()}"), true, sw.ElapsedTicks);
            CacheManager.Add(entityType, entity);
            return entity;
        }

        internal object HydrateObject(DataRow row, ClassMetadata metadata)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Εάν η γραμμή είναι null, θα πρέπει να επιστρέψουμε null - δεν υπάρχει συσχέτιση
            if (row == null) return null;

            //Δημιούργησε το νέο object 
            object entityBase = Activator.CreateInstance(metadata.EntityType);
            var columns = metadata.Columns.Select(c => c.Value)
                .Where(c => (c.IsCustomColumn == false))
                .Where(c => (c.RelationshipType != RelationshipType.OneToMany))
                .ToList();

            
            foreach (var column in columns)
            {
                object value = row[column.ColumnName];
                if (value == null || value == DBNull.Value)
                    continue;

                if (column.IsRelationship && column.RelationshipType == RelationshipType.ManyToOne)
                {
                    //Note to future self:
                    //Ενώ, κανονικά, δένουμε το ManyToOne με το primaryKey, εδώ έχει πάρει και έχει γαμηθεί.
                    //Οπότε δεν μπορούμε να πάμε να ψάξουμε με FindById, αλλά με FindOneBy και το 
                    //field στο οποίο κάνουμε reference
                    var targetObject = FindOneBy(column.TargetEntity, new Dictionary<string, object>()
                    {
                        {column.RelationshipReferenceColumn, value }
                    });
                    column.FieldInfo.SetValue(entityBase, targetObject);
                }
                else
                    column.FieldInfo.SetValue(entityBase, value);
            }
            if (metadata.HasCustomColumns())
            {
                HydrateCustomColumns(ref entityBase, metadata);
            }
            sw.Stop();
            OnOperationComplete(GetOperationName($"\t ObjectHydration: "), true, sw.ElapsedTicks);

            return entityBase;
        }

        private void HydrateCustomColumns(ref object entityBase, ClassMetadata metadata)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (metadata == null) throw new ArgumentNullException(nameof(metadata));

            string Query = CreateQueryBuilder()
                .Select(metadata.CustomReferenceColumn, "CustomFieldId", "CustomFieldValue")
                .From(metadata.CustomTable)
                .Where(new Eq(metadata.CustomReferenceColumn, "@identifier"))
                .GetQuery();

            DataTable table = new DataTable();
            using (SqlConnection Connection = CreateSqlConnection())
            {
                try
                {
                    Connection.Open();
                    using(Statement stmt = new Statement(Query, Connection))
                    {
                        table = stmt.BindParameter("@identifier", metadata.GetIdentifierField().GetValue(entityBase))
                            .Fetch();
                    }
                    Connection.Close();
                } catch (SqlException ex)
                {
                    sw.Stop();
                    OnOperationComplete(GetOperationName($"\t CustomColumHydration Failure: "), false, sw.ElapsedTicks);
                }
            }

            if (table == null)
                return;

            foreach (DataRow row in table.Rows)
            {
                object value = row["CustomFieldValue"];
                if (value == null || value == DBNull.Value)
                    continue;

                int columnId = (int)row["CustomFieldId"];
                try
                {
                    value = ConvertCustomColumn(metadata.GetCustomColumnMetadata(columnId), value);
                }
                catch (MetadataException mex)
                {
                    value = null;
                }                
                metadata.GetCustomColumnFieldInfo(columnId).SetValue(entityBase, value);
            }
            sw.Stop();
            OnOperationComplete(GetOperationName($"\tCustomColumnHydration Complete: "), true, sw.ElapsedTicks);
        }
        private bool ConvertStringToBoolean(string value)
        {
            int boolVal = 0;
            bool isInt = int.TryParse(value, out boolVal);
            if (!isInt)
            {
                return Convert.ToBoolean(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                return Convert.ToBoolean(boolVal, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        internal object ConvertCustomColumn(ColumnMetadata meta, object value)
        {
            if (value == null)
                return null;

            if (meta.ColumnType == typeof(SqlBoolean))
            {
                return ConvertStringToBoolean(value.ToString());
            }
            else if (meta.ColumnType == typeof(SqlByte))
            {
                return Convert.ToByte(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (meta.ColumnType == typeof(SqlDateTime))
            {
                return DateTimeHelper.ConvertStringToDatetime(value.ToString());
            }
            else if (meta.ColumnType == typeof(SqlDecimal))
            {
                return Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (meta.ColumnType == typeof(SqlDouble))
            {
                return Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (meta.ColumnType == typeof(SqlInt16))
            {
                return Convert.ToInt16(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (meta.ColumnType == typeof(SqlInt32))
            {
                return Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (meta.ColumnType == typeof(SqlInt64))
            {
                return Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (meta.ColumnType == typeof(SqlSingle))
            {
                return Convert.ToSingle(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (meta.ColumnType == typeof(SqlString))
            {
                return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            }

            return null;

        }

        public T FindOneBy<T>(Dictionary<string, object> parameters) where T : class
        {
            return FindBy<T>(parameters).FirstOrDefault();
        }

        public object FindOneBy(Type entityType, Dictionary<string, object> parameters)
        {
            return FindBy(entityType, parameters).FirstOrDefault();
        }

        public List<T> FindBy<T>(Dictionary<string, object> parameters) where T: class
        {
            ClassMetadata metadata = MetadataCache.Get<T>();

            var Query = CreateQueryBuilder()
                .Select(metadata.IdentifierColumn)
                .From(metadata.TableName);

            int currentParam = 0;
            if (parameters != null && parameters.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    if (currentParam == 0)
                        Query = Query.Where(new Eq(parameter.Key, $"@{parameter.Key}"));
                    else
                        Query = Query.AndWhere(new Eq(parameter.Key, $"@{parameter.Key}"));
                    currentParam++;
                }
            }
            DataTable dt = new DataTable();
            using (SqlConnection Connection = CreateSqlConnection())
            {
                try
                {
                    Connection.Open();
                    using (Statement stmt = new Statement(Query.GetQuery(), Connection))
                    {
                        dt = stmt.BindParameters(parameters)
                            .Fetch();
                    }
                    Connection.Close();
                }
                catch (SqlException ex)
                {
                    //TODO: Handle Exception
                }
            }
            List<T> objects = new List<T>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    objects.Add(FindById<T>(row[0]));
                }
            }
            return objects;
        }

        public List<object> FindBy(Type entityType, Dictionary<string, object> parameters)
        {
            ClassMetadata metadata = MetadataCache.Get(entityType);

            var Query = CreateQueryBuilder()
                .Select(metadata.IdentifierColumn)
                .From(metadata.TableName);

            int currentParam = 0;
            if (parameters != null && parameters.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    if (currentParam == 0)
                        Query = Query.Where(new Eq(parameter.Key, $"@{parameter.Key}"));
                    else
                        Query = Query.AndWhere(new Eq(parameter.Key, $"@{parameter.Key}"));
                    currentParam++;
                }
            }
            DataTable dt = new DataTable();
            using (SqlConnection Connection = CreateSqlConnection())
            {
                try
                {
                    Connection.Open();
                    using (Statement stmt = new Statement(Query.GetQuery(), Connection))
                    {
                        dt = stmt.BindParameters(parameters)
                            .Fetch();
                    }
                    Connection.Close();
                } catch (SqlException ex)
                {
                    //TODO: Handle Exception
                }
            }
            List<object> objects = new List<object>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    objects.Add(FindById(entityType, row[0]));
                }
            }
            return objects;
        }

        public List<T> FindAll<T>() where T : class
        {
            ClassMetadata metadata = MetadataCache.Get<T>();

            string Query = CreateQueryBuilder()
                .Select(metadata.IdentifierColumn)
                .From(metadata.TableName)
                .GetQuery();

            DataTable dt = new DataTable();

            using (SqlConnection Connection = CreateSqlConnection())
            {
                try
                {
                    Connection.Open();
                    using (Statement stmt = new Statement(Query, Connection))
                    {
                        dt = stmt.Fetch();
                    }
                    Connection.Close();
                }
                catch (SqlException ex)
                {
                    //TODO: Handle error
                }
            }
            List<T> objects = new List<T>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    objects.Add(FindById<T>(row[0]));
                }
            }
            return objects;
        }

        public List<object> FindAll(Type entityType)
        {
            ClassMetadata metadata = MetadataCache.Get(entityType);

            string Query = CreateQueryBuilder()
                .Select(metadata.IdentifierColumn)
                .From(metadata.TableName)
                .GetQuery();

            DataTable dt = new DataTable();

            using (SqlConnection Connection = CreateSqlConnection())
            {
                try
                {
                    Connection.Open();
                    using (Statement stmt = new Statement(Query, Connection))
                    {
                        dt = stmt.Fetch();
                    }
                    Connection.Close();
                } catch (SqlException ex)
                {
                    //TODO: Handle error
                }
            }
            List<object> objects = new List<object>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    objects.Add(FindById(entityType, row[0]));
                }
            }
            return objects;

        }

        public void Delete<T>(T entityObject) where T : class
        {
            throw new NotImplementedException();
        }

        public void Delete(Type entityType, object entityObject)
        {
            throw new NotImplementedException();
        }

        public DataTable GetResult(string query, Dictionary<string, object> parameters)
        {
            DataTable dataTable = null;
            using (SqlConnection Connection = CreateSqlConnection())
            {
                Connection.Open();
                using (Statement stmt = new Statement(query, Connection))
                {
                    dataTable = stmt.BindParameters(parameters)
                        .Fetch();
                }
                Connection.Close();
            }
            return dataTable;
        }

        public DataRow GetSingleResult(string query, Dictionary<string, object> parameters)
        {
            DataRow dataRow = null;
            using (SqlConnection Connection = CreateSqlConnection()) 
            {
                Connection.Open();
                using (Statement stmt = new Statement(query, Connection))
                {
                    dataRow = stmt.BindParameters(parameters)
                        .FetchRow();
                }
                Connection.Close();
            }
            return dataRow;
        }

        public object GetSingleScalarResult(string query, Dictionary<string, object> parameters)
        {
            object value = null;
            using (SqlConnection Connection = CreateSqlConnection())
            {
                Connection.Open();
                using (Statement stmt = new Statement(query, Connection))
                {
                    value = stmt.BindParameters(parameters)
                        .FetchScalar();
                }
                Connection.Close();
            }
            return value;
        }
    }
}
