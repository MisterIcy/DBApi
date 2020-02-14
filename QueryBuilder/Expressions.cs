using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBApi.Reflection;
namespace DBApi.QueryBuilder
{
    public abstract class Expression
    {
        public int Priority { get; set; }
        protected string PreSeperator { get; set; } = "(";
        protected string Seperator { get; set; } = ", ";
        protected string PostSeperator { get; set; } = ")";
#pragma warning disable CA2227 // Collection properties should be read only
        protected List<Expression> Expressions { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        public Expression() : this(0){ }
        public Expression(int Priority)
        {
            this.Priority = Priority;
            this.Expressions = new List<Expression>();
        }

        public int Count() => this.Expressions.Count;

        public override string ToString()
        {
            if (Count() == 1)
                return Expressions[0].ToString();

            StringBuilder strb = new StringBuilder(PreSeperator);
            foreach (Expression expr in Expressions)
            {
                strb.Append(expr.ToString())
                    .Append(Seperator);
            }
            if (strb.ToString().EndsWith(Seperator, StringComparison.InvariantCulture))
                strb.Remove(strb.Length - Seperator.Length, Seperator.Length);

            strb.Append(PostSeperator);
            return strb.ToString();
        }
    }
    #region Update
    public class Update : Expression
    {
        private readonly List<string> Fields;
        private readonly string TableName;
        private readonly string Alias;

        public Update (string  TableName, List<string> fields, string Alias = "t")
            :base(90)
        {
            this.TableName = TableName;
            this.Alias = Alias;
            this.Fields = fields;
        }
        public Update(Type EntityType)
            :base(90)
        {
            ClassMetadata meta = new ClassMetadata(EntityType);
            this.TableName = meta.TableName;
            this.Alias = "t";
            this.Fields = new List<string>();

            foreach (var column in meta.Columns)
            {
                if (column.Value.IsCustomColumn == true || column.Value.RelationshipType == RelationshipType.OneToMany)
                    continue;

                this.Fields.Add(column.Value.ColumnName);
            }
        }
        internal Update(ClassMetadata metadata)
            :base(90)
        {
            this.TableName = metadata.TableName;
            this.Alias = "t";
            this.Fields = new List<string>();

            var columns = metadata.Columns.Select(c => c.Value)
                .ToList();
            foreach (var column in columns)
            {
                if (column.IsIdentifier)
                    continue;
                if (column.IsCustomColumn)
                    continue;
                if (column.IsRelationship && column.RelationshipType == RelationshipType.OneToMany)
                    continue;

                this.Fields.Add(column.ColumnName);
            }
            
        }
        public override string ToString()
        {
            StringBuilder strb = new StringBuilder("UPDATE " + TableName + " SET ");
            foreach (string field in Fields)
            {
                strb.Append(field + " = @" + field + Seperator);
            }
            if (strb.ToString().EndsWith(Seperator, StringComparison.InvariantCulture))
                strb.Remove(strb.Length - Seperator.Length, Seperator.Length);

            strb.Append(" ");
            return strb.ToString();
        }
    }

    #endregion
    #region Delete
    public class Delete : Expression
    {
        private readonly string Table;
        private readonly string Alias;
        public bool WithOutput { get; set; } = false;
        public Delete(string Table, string Alias = "t")
        {
            this.Table = Table;
            this.Alias = Alias;
        }
        public Delete(Type EntityType, string Alias = "t") 
        {
            ClassMetadata meta = new ClassMetadata(EntityType);
            this.Table = meta.TableName;
            this.Alias = Alias;

        }
        public override string ToString()
        {
            return new StringBuilder("DELETE FROM ")
                .Append(Table + " " + Alias + " ")
                .ToString();
        }
    }
    #endregion

    #region Insert
    public class InsertInto : Expression
    {
        private readonly List<string> Fields;
        public bool WithOutput { get; set; } = false;
        private readonly string Table;
        private readonly string Alias;
        private readonly string Identifier;

        public InsertInto(string TableName, string Alias = "t", bool Output = false, string OutputWhat="*", params string[] fields)
            :base(90)
        {
            this.Table = TableName;
            this.Alias = Alias;
            this.WithOutput = Output;
            this.Identifier = OutputWhat;
            this.Fields = fields.ToList<string>();
        }

        public InsertInto(Type EntityType, bool Output = false)
            :base(90)
        {
            ClassMetadata meta = new ClassMetadata(EntityType);
            this.Table = meta.TableName;
            this.Alias = "t";
            this.WithOutput = Output;
            this.Identifier = meta.IdentifierColumn;
            this.Fields = new List<string>();
            foreach (var column in meta.Columns)
            {
                if (column.Value.IsIdentifier)
                    continue;
                if (column.Value.IsCustomColumn || column.Value.RelationshipType == RelationshipType.OneToMany)
                    continue;

                this.Fields.Add(column.Value.ColumnName);
            }
        }
        public override string ToString()
        {
            StringBuilder strb = new StringBuilder("INSERT INTO " + Table + " " + PreSeperator);
            foreach (string field in Fields)
            {
                strb.Append(field + Seperator);
            }
            if (strb.ToString().EndsWith(Seperator, StringComparison.InvariantCulture))
                strb.Remove(strb.Length - Seperator.Length, Seperator.Length);

            strb.Append(PostSeperator);
            if (WithOutput)
            {
                strb.Append(" OUTPUT INSERTED." + Identifier);
            }
            strb.Append(" VALUES" + PreSeperator);
            foreach (string field in Fields)
            {
                strb.Append("@" + field + Seperator);
            }
            if (strb.ToString().EndsWith(Seperator, StringComparison.InvariantCulture))
                strb.Remove(strb.Length - Seperator.Length, Seperator.Length);
            strb.Append(PostSeperator).Append(" ");

            return strb.ToString();

        }
    }
    
    #endregion
    #region And/Or 
    public class AndX : Expression
    {
        private const string AND = " AND ";
        private readonly List<Operation> Operations;

        public AndX(Operation Operation)
        {
            this.Operations = new List<Operation>()
            {
                Operation
            };
        }
        public AndX(params Operation[] Operations)
        {
            this.Operations = Operations.ToList<Operation>();
        }
        public override string ToString()
        {
            StringBuilder strb = new StringBuilder();
            foreach (var Operation in Operations)
            {
                strb.Append(Operation).Append(AND);
            }
            if (strb.ToString().EndsWith(AND, StringComparison.InvariantCulture))
                strb.Remove(strb.Length - AND.Length, AND.Length);

            return strb.ToString();
        }
    }
    public class OrX: Expression
    {
        private const string OR = " OR ";
        private readonly List<Operation> Operations;

        public OrX(Operation operation)
        {
            this.Operations = new List<Operation>()
            {
                operation
            };
        }
        public OrX(params Operation[] Operations)
        {
            this.Operations = Operations.ToList<Operation>();
        }

        public override string ToString()
        {
            StringBuilder strb = new StringBuilder();
            foreach (var Operation in Operations)
            {
                strb.Append(Operation).Append(OR);
            }
            if (strb.ToString().EndsWith(OR, StringComparison.InvariantCulture))
                strb.Remove(strb.Length - OR.Length, OR.Length);

            return strb.ToString();
        }

    }
    #endregion
    #region Select From 
    
    public class Select: Expression
    {
        private readonly List<string> fields;
        private int top = 0;

        internal Select(ClassMetadata meta)
            :base (90)
        {
            this.PreSeperator = "";
            this.PostSeperator = "";
            this.fields = new List<string>();

            foreach (var column in meta.Columns)
            {
                if (column.Value.IsCustomColumn || (
                    column.Value.IsRelationship && column.Value.RelationshipType == RelationshipType.OneToMany))
                    continue;
                this.fields.Add(column.Value.ColumnName);
            }
        }
        public Select() :
            base(90)
        {
            this.PreSeperator = "";
            this.PostSeperator = "";
            this.fields = new List<string>();
        }

        public Select(params string[] fields)
            :base(90)
        {
            this.PreSeperator = "";
            this.PostSeperator = "";
            this.fields = fields.ToList<string>();
        }
        public Select(Type EntityType)
            :base(90)
        {
            this.fields = new List<string>();
            ClassMetadata meta = new ClassMetadata(EntityType);
            foreach (var column in meta.Columns)
            {
                this.fields.Add(column.Value.ColumnName);
            }
        }
        public void SetLimit(int limit)
        {
            this.top = limit;
        }
        public override string ToString()
        {
            StringBuilder strb = new StringBuilder("SELECT ");
            if (top >0)
            {
                strb.Append("TOP (" + top + ")");
            }
            if (fields.Count == 0)
            {
                strb.Append("* ");
                return strb.ToString();
            }
            foreach (string field in fields)
            {
                strb.Append(field + Seperator);
            }
            if (strb.ToString().EndsWith(Seperator, StringComparison.InvariantCulture))
                strb.Remove(strb.Length - Seperator.Length, Seperator.Length);

            strb.Append(" ");
            return strb.ToString();
        }

    }
    public class From : Expression
    {
        private readonly string TableName;
        private readonly string Alias;

        internal From(ClassMetadata meta) 
            :base(80)
        {
            this.Alias = "t";
            this.TableName = meta.TableName;
        }
        public From(Type Type)
            :base(80)
        {
            this.Alias = "t";
            this.TableName = MetadataCache.Get(Type).TableName;
        }
        public From(string TableName, string Alias = "t")
            :base(80)
        {
            this.Alias = Alias;
            this.TableName = TableName;
        }
        public From(QueryBuilder Query, string Alias = "c")
        {
            this.TableName = "(" + Query.GetQuery() + ")";
            this.Alias = Alias;
        }
        public override string ToString()
        {
            StringBuilder strb = new StringBuilder("FROM ")
                .Append(TableName)
                .Append(" ")
                .Append(Alias)
                .Append(" ");

            return strb.ToString();
        }
    }
    public  class Where :Expression
    {
        private readonly bool isAnd = false;
        private readonly bool isOr = false;

        public Where(Expression expression, bool isAnd = false, bool isOr = false)
            :base (50)
        {
            this.Expressions = new List<Expression>()
            {
                expression
            };
            this.isAnd = isAnd;
            this.isOr = isOr;
        }
        public override string ToString()
        {
            StringBuilder strb = new StringBuilder();
            if (isAnd)
                strb.Append(" AND (");
            else if (isOr)
                strb.Append(" OR (");
            else
                strb.Append(" WHERE ");

            strb.Append(base.ToString());

            if (isAnd || isOr)
                strb.Append(")");

            strb.Append(" ");
            return strb.ToString();


                
        }
    }
    public class GroupBy : Expression
    {
        private readonly string Field;
        public GroupBy(string field) : base(20)
        {
            this.Field = field;
        }
        public override string ToString()
        {
            return new StringBuilder("GROUP BY ")
                .Append(Field).Append(" ").ToString();
        }
    }
    public class Having: Expression
    {
        private readonly string Condition;
        public Having(string Condition) : base(30)
        {
            this.Condition = Condition;
        }
        public Having(Operation Operation)
        {
            this.Condition = "COUNT(" + Operation.ToString() + ")";
        }
        public override string ToString()
        {
            return new StringBuilder("HAVING ")
                .Append(Condition)
                .Append(" ")
                .ToString();
        }
    }
    public class OrderBy : Expression
    {
        private readonly string field = string.Empty;
        private readonly string order = "ASC";
        public OrderBy(string field, string order = "ASC")
            :base (10)
        {
            this.field = field;
            this.order = order;
        }
        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "ORDER BY {0} {1}", this.field, this.order);
        }
    }
    #endregion
    #region Join Expressions
    public abstract class Join : Expression
    {
        private readonly string JoinType = "JOIN";
        private readonly string TableName = string.Empty;
        private readonly string TableAlias = string.Empty;
        private readonly Operation Operation;

        public override string ToString()
        {
            return new StringBuilder(JoinType + " ")
                .Append(TableName + " ")
                .Append(TableAlias + " ON ")
                .Append(Operation.ToString() + " ")
                .ToString();
        }
        public Join(string JoinType, string TableName, string TableAlias, Operation operation)
            :base(70)
        {
            this.JoinType = JoinType;
            this.TableAlias = TableAlias;
            this.TableName = TableName;
            this.Operation = operation;

        }
    }
    public sealed class SimpleJoin : Join
    {
        public SimpleJoin(string TableName, string TableAlias, Operation Operation)
            :base("JOIN", TableName, TableAlias, Operation) { }
    }
    public sealed class InnerJoin: Join
    {
        public InnerJoin(string TableName, string TableAlias, Operation Operation)
            : base("INNER JOIN", TableName, TableAlias, Operation) { }
    }
    public sealed class LeftJoin : Join
    {
        public LeftJoin(string TableName, string TableAlias, Operation Operation)
            : base("LEFT JOIN", TableName, TableAlias, Operation) { }
    }
    public sealed class RightJoin : Join
    {
        public RightJoin(string TableName, string TableAlias, Operation Operation)
            : base("RIGHT JOIN", TableName, TableAlias, Operation) { }
    }
    public sealed class FullJoin : Join
    {
        public FullJoin(string TableName, string TableAlias, Operation Operation)
            : base("FULL JOIN", TableName, TableAlias, Operation) { }
    }

    #endregion
    #region Operations
    public abstract class Operation : Expression
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";
        private readonly object LeftOperand;
        private readonly object RightOperand;
        protected string Operator { get; set; } = string.Empty;

        public Operation(object Left, object Right)
        {
            this.LeftOperand = Left;
            this.RightOperand = Right;
        }

        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0} {1} {2}",
                GetOperand(LeftOperand),
                Operator,
                GetOperand(RightOperand));
        }

        private static string GetOperand (object Operand)
        {
            if (Operand is DateTime)
            {
                return "'" + ((DateTime)Operand)
                    .ToString(DateFormat, System.Globalization.CultureInfo.InvariantCulture);
            }
            return Operand.ToString();
        }
        protected string Left { get => GetOperand(this.LeftOperand); }
        protected string Right {get => GetOperand(this.RightOperand); }
    }
    public sealed class Eq: Operation
    {
        public Eq(object left, object right)
            :base(left, right)
        {
            this.Operator = "=";
        }
    }
    public sealed class Neq: Operation
    {
        public Neq(object left, object right) 
            :base (left, right)
        {
            this.Operator = "!=";
        }
    }
    public sealed class Gt: Operation
    {
        public Gt(object left, object right)
            : base(left, right)
        {
            this.Operator = ">";
        }
    }
    public sealed class Gte : Operation
    {
        public Gte(object left, object right)
            : base(left, right)
        {
            this.Operator = ">=";
        }
    }
    public sealed class Lt : Operation
    {
        public Lt(object left, object right)
            : base(left, right)
        {
            this.Operator = "<";
        }
    }
    public sealed class Lte: Operation
    {
        public Lte(object left, object right)
            : base(left, right)
        {
            this.Operator = "<=";
        }
    }
    public sealed class Ngt : Operation
    {
        public Ngt(object left, object right)
            : base(left, right)
        {
            this.Operator = "!>";
        }
    }
    public sealed class Nlt : Operation
    {
        public Nlt(object left, object right)
            : base(left, right)
        {
            this.Operator = "!<";
        }
    }
    public sealed class IsNull : Operation
    {
        public IsNull(object left)
            : base(left, null)
        {
            this.Operator = "IS NULL";
        }
        public override string ToString()
        {
            return Left + " " + Operator + " ";
        }
    }
    public sealed class IsNotNull : Operation
    {
        public IsNotNull(object left)
            : base(left, null)
        {
            this.Operator = "IS NOT NULL";
        }
        public override string ToString()
        {
            return Left + " " + Operator + " ";
        }
    }
    public sealed class LikeOp: Operation
    {
        public LikeOp(string field, object term)
            :base (field, term)
        {
            this.Operator = "LIKE";
        }
    }
    public sealed class InOp : Operation
    {
        private readonly List<object> values;
        public InOp(string field, params object[] values)
            :base(field, null)
        {
            this.Seperator = ", ";
            this.Operator = "IN";
            this.values = values.ToList<object>();
        }

        public override string ToString()
        {
            StringBuilder strb = new StringBuilder(Left)
                .Append(" " + Operator + " ");

            foreach (var value in values)
            {
                strb.Append(value.ToString()).Append(Seperator);
            }
            if (strb.ToString().EndsWith(Seperator, StringComparison.InvariantCulture))
                strb.Remove(strb.Length - Seperator.Length, Seperator.Length);

            strb.Append(" ");
            return strb.ToString();
        }
    }
    public class Between : Operation
    {
        private readonly string field;

        public Between(string field, object start, object end)
            :base(start, end)
        {
            this.field = field;
            this.Operator = "BETWEEN";
        }
        public override string ToString()
        {
            return new StringBuilder(field)
                .Append(" " + this.Operator)
                .Append(" " + this.Left)
                .Append(" AND " + this.Right)
                .Append(" ")
                .ToString();
        }
    }
    #endregion

    #region Transactions
    public  abstract class TransactionExpression : Expression
    {
        protected string TransactionId { get; set; }
        protected string Operation { get; set; }

        public TransactionExpression(int Priority)
            :base(Priority) { }
        public override string ToString()
        {
            return Operation + 
                ((string.IsNullOrEmpty(TransactionId)) ? "" : TransactionId) + ";";
        }
    }
    public class BeginTransaction : TransactionExpression
    {

        public BeginTransaction(string TransactionId = null)
            : base(200)
        {
            this.Operation = "BEGIN TRAN ";
            this.TransactionId = string.IsNullOrEmpty(TransactionId) ?
                Guid.NewGuid().ToString().ToUpper(System.Globalization.CultureInfo.InvariantCulture).Substring(0, 8) :
                TransactionId;
        }        
    }
    public class CommitTransaction : TransactionExpression {
        public CommitTransaction(string TransactionId = null)
        :base(-100)
        {
            this.Operation = "COMMIT TRAN ";
            this.TransactionId = string.IsNullOrEmpty(TransactionId) ?
                Guid.NewGuid().ToString().ToUpper(System.Globalization.CultureInfo.InvariantCulture).Substring(0, 8) :
                TransactionId;
        }
    }
    public class RollbackTransaction: TransactionExpression
    {
        public RollbackTransaction(string TransactionId = null)
            :base(-100)
        {
            this.Operation = "ROLLBACK TRAN ";
            this.TransactionId = string.IsNullOrEmpty(TransactionId) ?
                Guid.NewGuid().ToString().ToUpper(System.Globalization.CultureInfo.InvariantCulture).Substring(0, 8) :
                TransactionId;
        }
    }

    #endregion

    public class ExpressionPriority : IComparer<Expression>
    {
        public int Compare(Expression x, Expression y)
        {
            if (x.Priority == 0 || y.Priority == 0)
                return 0;
            
            return y.Priority.CompareTo(x.Priority);
        }
    }

}
