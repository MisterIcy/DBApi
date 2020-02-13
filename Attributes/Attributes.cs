using System;

#pragma warning disable CA1721 // Property names should not match get methods
namespace DBApi.Attributes
{
    /// <summary>
    /// Επισημαίνει την κλάση ως οντότητα
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EntityAttribute : Attribute {
        public EntityAttribute() { }
    }

    /// <summary>
    /// Καθορίζει τον πίνακα της βάσης δεδομένων που είναι συσχετισμένος με την οντότητα
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TableAttribute : Attribute
    {        
        public TableAttribute(string TableName)
        {
            if (string.IsNullOrEmpty(TableName))
                throw new ArgumentNullException(nameof(TableName));

            this.TableName = TableName;
        }
        /// <summary>
        /// Όνομα πίνακα στην βάση δεδομένων
        /// </summary>
        public string TableName { get; private set; }
    }

    /// <summary>
    /// Επισημαίνει το πεδίο ως πρωτεύον κλειδί της οντότητας
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class IdentityAttribute : Attribute
    {
        public IdentityAttribute() { }
    }
    /// <summary>
    /// Επισημαίνει το πεδίο ώς μοναδικό αναγνωριστικό (GUID) της οντότητας
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GuidAttribute : Attribute
    {
        public GuidAttribute() { }
    }
    /// <summary>
    /// Επισημαίνει πως το πεδίο συσχετίζεται με συγκεκριμένη στήλη του πίνακα
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string Name, ColumnType ColumnType = ColumnType.String, bool Nullable = true, bool Unique = false)
        {
            if (string.IsNullOrEmpty(Name))
                throw new ArgumentNullException(nameof(Name));

            this.ColumnName = Name;
            this.ColumnType = ColumnType;
            this.Nullable = Nullable;
            this.Unique = Unique;
        }
        /// <summary>
        /// Όνομα στηλης στον πίνακα
        /// </summary>
        public string ColumnName { get; private set; }
        /// <summary>
        /// Τύπος στήλης
        /// </summary>
        public ColumnType ColumnType { get; private set; }
        /// <summary>
        /// Δέχεται NULL τιμες;
        /// </summary>
        public bool Nullable { get; private set; }
        /// <summary>
        /// Είναι μοναδικό;
        /// </summary>
        public bool Unique { get; private set; }
    }

    /// <summary>
    /// Επισημαίνει πως το πεδίο συσχετίζεται με "παραμετρικό πεδίο" σε πίνακα εκτός οντότητας
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CustomColumnAttribute : Attribute
    {
        public CustomColumnAttribute(string TableName, string IdentifierColumn, int ColumnFieldId, ColumnType ColumnType = ColumnType.String)
        {
            if (string.IsNullOrEmpty(TableName))
                throw new ArgumentNullException(nameof(TableName));

            this.CustomTableName = TableName;
            if (string.IsNullOrEmpty(IdentifierColumn))
                throw new ArgumentNullException(nameof(IdentifierColumn));
            this.IdentifierColumn = IdentifierColumn;
            this.CustomFieldId = ColumnFieldId;
            this.ColumnType = ColumnType;
        }
        /// <summary>
        /// Ο πίνακας όπου αποθηκεύονται τα παραμετρικά
        /// </summary>
        public string CustomTableName { get; private set; }
        /// <summary>
        /// Η στήλη που "συσχετίζει" το παραμετρικό πεδίο με την οντότητα στην οποία ανήκει
        /// </summary>
        public string IdentifierColumn { get; private set; }
        /// <summary>
        /// Το αναγνωριστικό του παραμετρικού πεδίου
        /// </summary>
        public int CustomFieldId { get; private set; }
        /// <summary>
        /// Τύπος πεδίου
        /// </summary>
        public ColumnType ColumnType { get; private set; }
    }
    /// <summary>
    /// Καθορίζει την αποθήκευση αντικειμένων στην λανθάνουσα μνήμη
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CacheControlAttribute : Attribute
    {
        /// <summary>
        /// Χρόνος παραμονής στην λανθάνουσα μνήμη, σε δευτερόλεπτα
        /// </summary>
        public long Duration { get; private set; } = 3600;
        /// <summary>
        /// True εάν δεν θα πρέπει να αποθηκεύουμε ποτέ την ονότητα στη λανθάνουσα μνήμη
        /// </summary>
        public bool NoCache { get; private set; } = false;
        public CacheControlAttribute(long Duration  = 3600, bool NoCache = false)
        {
            this.Duration = Duration;
            this.NoCache = NoCache;
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
        public Type TargetEntity { get; private set; }
        /// <summary>
        /// Πεδίο όπου υπάρχει το αναγνωριστικό, ώστε να είναι εφικτή η συσχέτιση
        /// </summary>
        public string IdentifierColumn { get; private set; }

        public ManyToOneAttribute(Type TargetEntity, string Identifier)
        {
            this.TargetEntity = TargetEntity ?? throw new ArgumentNullException(nameof(TargetEntity));
            
            if (string.IsNullOrEmpty(Identifier))
                throw new ArgumentNullException(nameof(Identifier));
            
            this.IdentifierColumn = Identifier;
        }
    }
    /// <summary>
    /// Επισημαίνει πως το πεδίο έχει μια συσχέτιση ένα προς πολλά
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class OneToManyAttribute : Attribute
    {
        /// <summary>
        /// Τύπος οντότητας που είναι συνδεδεμένος με την παρούσα οντότητα
        /// </summary>
        public Type TargetEntity { get; private set; }
        /// <summary>
        /// Πεδίο όπου υπάρχει το αναγνωριστικό, ώστε να είναι εφικτή η συσχέτιση
        /// </summary>
        public string IdentifierColumn { get; private set; }

        public OneToManyAttribute(Type targetEntity, string referencedColumn)
        {
            this.TargetEntity = targetEntity ?? throw new ArgumentNullException(nameof(targetEntity));

            if (string.IsNullOrEmpty(referencedColumn))
                throw new ArgumentNullException(nameof(referencedColumn));

            IdentifierColumn = referencedColumn;
        }
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
