using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DBApi.Annotations;

namespace DBApi.Attributes
{
    /// <summary>
    /// Declares that the annotated class is a Database Entity
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [PublicAPI]
    public sealed class EntityAttribute : Attribute { }

    /// <summary>
    /// Adds a reference to the annotated class that shows the database table which is related to this entity
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [PublicAPI]
    public sealed class TableAttribute : Attribute
    {        
        public TableAttribute(string tableName)
        {
            TableName = string.IsNullOrEmpty(tableName)
                ? throw new ArgumentNullException(nameof(tableName))
                : tableName;
        }
        /// <summary>
        /// Gets a value that indicates the table that is associated to the annotated entity
        /// </summary>
        [NotNull] public string TableName { get; }
    }

    /// <summary>
    /// Declares the annotated field as a Table Identifier (Primary Key)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [PublicAPI]
    public sealed class IdentityAttribute : Attribute { }
    /// <summary>
    /// Declares the annotated field as a GUID Column
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [PublicAPI]
    public sealed class GuidAttribute : Attribute
    {
    }
    /// <summary>
    /// Επισημαίνει πως το πεδίο συσχετίζεται με συγκεκριμένη στήλη του πίνακα
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [PublicAPI]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string name, ColumnType columnType = ColumnType.String, bool nullable = true, bool unique = false)
        {
            ColumnName = string.IsNullOrEmpty(name) ? throw new ArgumentNullException(nameof(name)) : name;
            ColumnType = columnType;
            Nullable = nullable;
            Unique = unique;
        }
        /// <summary>
        /// Gets a value that defines the column associated with this field
        /// </summary>
        public string ColumnName { get; }
        /// <summary>
        /// Gets a value that defines the type of the column
        /// </summary>
        public ColumnType ColumnType { get; }
        /// <summary>
        /// Gets a value that defines whether the field accepts null values or not
        /// </summary>
        public bool Nullable { get; }
        /// <summary>
        /// Gets a value that defines whether the field accepts unique values or not
        /// </summary>
        public bool Unique { get; }
    }

    /// <summary>
    /// Επισημαίνει πως το πεδίο συσχετίζεται με "παραμετρικό πεδίο" σε πίνακα εκτός οντότητας
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [PublicAPI]
    public sealed class CustomColumnAttribute : Attribute
    {
        public CustomColumnAttribute(string tableName, string identifierColumn, int columnFieldId, ColumnType columnType = ColumnType.String)
        {
            CustomTableName = string.IsNullOrEmpty(tableName)
                ? throw new ArgumentNullException(nameof(tableName))
                : tableName;
            
            IdentifierColumn = identifierColumn;
            CustomFieldId = columnFieldId;
            ColumnType = columnType;
        }
        /// <summary>
        /// Ο πίνακας όπου αποθηκεύονται τα παραμετρικά
        /// </summary>
        public string CustomTableName { get; }
        /// <summary>
        /// Η στήλη που "συσχετίζει" το παραμετρικό πεδίο με την οντότητα στην οποία ανήκει
        /// </summary>
        public string IdentifierColumn { get; }
        /// <summary>
        /// Το αναγνωριστικό του παραμετρικού πεδίου
        /// </summary>
        public int CustomFieldId { get; }
        /// <summary>
        /// Τύπος πεδίου
        /// </summary>
        public ColumnType ColumnType { get; }
    }
    /// <summary>
    /// Καθορίζει την αποθήκευση αντικειμένων στην λανθάνουσα μνήμη
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [PublicAPI]
    public sealed class CacheControlAttribute : Attribute
    {
        /// <summary>
        /// Χρόνος παραμονής στην λανθάνουσα μνήμη, σε δευτερόλεπτα
        /// </summary>
        public long Duration { get; }
        /// <summary>
        /// True εάν δεν θα πρέπει να αποθηκεύουμε ποτέ την ονότητα στη λανθάνουσα μνήμη
        /// </summary>
        public bool NoCache { get; }
        public CacheControlAttribute(long duration  = 3600, bool noCache = false)
        {
            Duration = duration;
            NoCache = noCache;
        }
    }
    /// <summary>
    /// Επισημαίνει πως το πεδίο έχει μια συσχέτιση πολλών προς ένα
    /// </summary>
    [AttributeUsage(AttributeTargets.Field )]
    public sealed class ManyToOneAttribute : Attribute
    {
        /// <summary>
        /// Τύπος οντότητας που είναι συνδεδεμένος με την παρούσα οντότητα
        /// </summary>
        public Type TargetEntity { get; }
        /// <summary>
        /// Πεδίο όπου υπάρχει το αναγνωριστικό, ώστε να είναι εφικτή η συσχέτιση
        /// </summary>
        public string IdentifierColumn { get; }

        public ManyToOneAttribute(Type targetEntity, string identifier)
        {
            this.TargetEntity = targetEntity ?? throw new ArgumentNullException(nameof(targetEntity));
            
            IdentifierColumn = string.IsNullOrEmpty(identifier) ?
                throw new ArgumentNullException(nameof(identifier)) :
                IdentifierColumn = identifier;
        }
    }
    /// <summary>
    /// Επισημαίνει πως το πεδίο έχει μια συσχέτιση ένα προς πολλά
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [PublicAPI]
    public sealed class OneToManyAttribute : Attribute
    {
        /// <summary>
        /// Τύπος οντότητας που είναι συνδεδεμένος με την παρούσα οντότητα
        /// </summary>
        public Type TargetEntity { get; }
        /// <summary>
        /// Πεδίο όπου υπάρχει το αναγνωριστικό, ώστε να είναι εφικτή η συσχέτιση
        /// </summary>
        public string IdentifierColumn { get; }

        public OneToManyAttribute(Type targetEntity, string referencedColumn)
        {
            TargetEntity = targetEntity ?? throw new ArgumentNullException(nameof(targetEntity));

            IdentifierColumn = string.IsNullOrEmpty(referencedColumn)
                ? throw new ArgumentNullException(nameof(referencedColumn))
                : referencedColumn;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    [PublicAPI]
    public sealed class VersionAttribute : Attribute
    {
        
    }
    /// <summary>
    /// Enumeration με τους τύπους δεδομένων που μπορούμε να χρησιμοποιήσουμε στις οντότητες
    /// </summary>
    public enum ColumnType
    {

        Binary,
        Boolean,
        Byte,
        Bytes,
        Chars,
        DateTime,
        Decimal,
        Double,
        Guid,
        Int16,
        Int32,
        Int64,
        Money,
        Single,
        String,
        Xml,
        /** OLD TYPES, USED FOR BC */
        /// <summary>
        /// nchar & nvarchar
        /// </summary>
        STRING,
        /// <summary>
        /// Integer values
        /// </summary>
        INTEGER,
        /// <summary>
        /// Floating point values
        /// </summary>
        DOUBLE,
        /// <summary>
        /// true / false values
        /// </summary>
        BOOLEAN,
        /// <summary>
        /// DateTime
        /// </summary>
        DATETIME,
        /// <summary>
        /// Dates
        /// </summary>
        DATE,
        /// <summary>
        /// Times
        /// </summary>
        TIME
    }
}
