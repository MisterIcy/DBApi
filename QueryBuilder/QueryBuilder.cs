using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBApi.Reflection;
/// <summary>
/// Query Builder
/// </summary>
namespace DBApi.QueryBuilder
{
    /// <summary>
    /// Ο Πανίσχυρος Μεγιστάνας Των Βάσεων Δεδομένων
    /// </summary>
    /// <remarks>
    /// Version 0.4: Extension of Query Builder
    /// </remarks>
    public class QueryBuilder
    {
        private readonly EntityManager entityManager;

        private List<Expression> expressions;
        /// <summary>
        /// Δημιουργεί έναν νέο QueryBuilder
        /// </summary>
        public QueryBuilder()
        {
            expressions = new List<Expression>();
        }
        /// <summary>
        /// Δημιουργεί έναν νεο QueryBuilder
        /// </summary>
        /// <param name="em">Ο <see cref="EntityManager">Entity Manager</see> που θα χρησιμοποιήσουμε για να δημιουργήσουμε συνδέσεις στην βάση δεδομένων, κλπ.</param>
        /// <param name="forceTest">Εάν ο καλούντας περάσει true, δημιουργείται μια απλή σύνδεση με την βάση δεδομένων, ώστε να ελέγξουμε την συνδεσιμότητα με τον database server. By default, είναι false</param>
        /// <exception cref="SqlException">Δεν υπάρχει exception handling κατά το connection testing, οπότε, εάν αποτύχει η σύνδεση για κάποιο λόγο, θα πεταχτεί ένα exception</exception>
        public QueryBuilder (EntityManager em, bool forceTest = false)
            :this()
        {
            this.entityManager = em;
            if (forceTest)
            {
                using (SqlConnection connection = entityManager.CreateSqlConnection())
                {
                    connection.Open();
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// Δημιουργεί ένα Select Statement
        /// </summary>
        /// <remarks>Αντίστοιχο του SELECT *</remarks>
        /// <returns>Το τρέχον instance του Query Builder</returns>
        public QueryBuilder Select() {
            expressions.Add(new Select());
            return this;
        }
        /// <summary>
        /// Δημιουργεί ένα Select Statement
        /// </summary>
        /// <remarks>Αντίστοιχο του **SELECT t.field_1, t.field_2, ... t.field_n**
        /// </remarks>
        /// <returns>Το τρέχον instance του Query Builder</returns>
        public QueryBuilder Select(params string[] fields) {
            expressions.Add(new Select(fields));
            return this;
        }
        /// <summary>
        /// Δημιουργεί ένα Select Statement
        /// </summary>
        /// <param name="type">O τύπος της κλάσης / οντότητας την οποία θέλουμε να αντλήσουμε από την βάση δεδομένων</param>
        /// <returns>Το τρέχον instance του Query Builder</returns>
        public QueryBuilder Select(Type type) {
            expressions.Add(new Select(type));
            return this;
        }

        public QueryBuilder From(string TableName, string Alias = "t")
        {
            expressions.Add(new From(TableName, Alias));
            return this;
        }
        public QueryBuilder NestedFrom(QueryBuilder nestedQuery, string Alias = "t")
        {
            expressions.Add(new From(nestedQuery, Alias));
            return this;
        }
        public QueryBuilder From(Type type)
        {
            expressions.Add(new From(type));
            return this;
        }
        public QueryBuilder From<T>()
            where T: class
        {
            return From(typeof(T));
        }

        
            

        public QueryBuilder Where(Expression expression) {
            expressions.Add(new Where(expression));

            

            return this;
        }
        public QueryBuilder AndWhere(Expression expression)
        {
            var AndExpr = new Where(expression, true);
            AndExpr.Priority = 45;
            expressions.Add(AndExpr);
            return this;
        }
        public QueryBuilder OrWhere(Expression expression)
        {
            var OrExpr = new Where(expression, false, true);
            OrExpr.Priority = 45;

            expressions.Add(OrExpr);
            return this;
        }
        public QueryBuilder OrderBy(string field, string order = "ASC")
        {
            expressions.Add(new OrderBy(field, order));
            return this;
        }
        public QueryBuilder OrderAsc(string field)
        {
            return this.OrderBy(field, "ASC");
        }
        public QueryBuilder OrderDesc(string field)
        {
            return this.OrderBy(field, "DESC");
        }

        public QueryBuilder Join(string tableName, string tableAlias, Operation onOperation)
        {
            expressions.Add(new SimpleJoin(tableName, tableAlias, onOperation));
            return this;
        }

        public QueryBuilder InnerJoin(string tableName, string tableAlias, Operation onOperation)
        {
            expressions.Add(new InnerJoin(tableName, tableAlias, onOperation));
            return this;
        }

        public QueryBuilder LeftJoin(string tableName, string tableAlias, Operation onOperation)
        {
            expressions.Add(new LeftJoin(tableName, tableAlias, onOperation));
            return this;
        }
        public QueryBuilder RightJoin(string tableName, string tableAlias, Operation onOperation)
        {
            expressions.Add(new RightJoin(tableName, tableAlias, onOperation));
            return this;
        }

        public QueryBuilder GroupBy(string field)
        {
            expressions.Add(new GroupBy(field));
            return this;
        }
        public QueryBuilder Having(string condition)
        {
            expressions.Add(new Having(condition));
            return this;
        }
        public QueryBuilder Having(Operation operation)
        {
            expressions.Add(new Having(operation));
            return this;
        }
        public QueryBuilder Like(string field, object term)
        {
            expressions.Add(new LikeOp(field, term));
            return this;
        }
        public QueryBuilder Top(int rows)
        {
            foreach (var expression in expressions)
            {
                if (expression is Select)
                {
                    ((Select)expression).SetLimit(rows);
                }
            }
            return this;
        }
        public QueryBuilder Limit(int rows)
        {
            return Top(rows);
        }
        public string GetQuery() 
        {
            expressions.Sort(new ExpressionPriority());

            StringBuilder strb = new StringBuilder();
            foreach (Expression expression in expressions)
            {
                strb.Append(expression.ToString());
            }
            return strb.ToString();
        }
        public QueryBuilder Insert(Type EntityType, bool output = false)
        {
            this.expressions.Add(new InsertInto(EntityType, output));
            return this;
        }
        public QueryBuilder BeginTransaction(string TransactionId)
        {
            this.expressions.Add(new BeginTransaction(TransactionId));
            return this;
        }
        public QueryBuilder Commit(string TransactionId)
        {
            this.expressions.Add(new CommitTransaction(TransactionId));
            return this;
        }
        public QueryBuilder Rollback(string TransactionId)
        {
            this.expressions.Add(new RollbackTransaction(TransactionId));
            return this;
        }

        internal QueryBuilder SelectInternal(ClassMetadata metadata)
        {
            this.expressions.Add(new Select(metadata));
            return this;
        }
        internal QueryBuilder FromInternal(ClassMetadata metadata)
        {
            this.expressions.Add(new From(metadata));
            return this;
        }
        internal QueryBuilder UpdateInternal(ClassMetadata metadata)
        {
            this.expressions.Add(new Update(metadata));
            return this;
        }
    }
}
