using System;
using System.Reflection;
using DBApi.Attributes;
using System.Data.SqlTypes;
using System.Collections.Generic;

namespace DBApi.Reflection
{
    /// <summary>
    /// Κρατάει τα μεταδεδομένα των στηλών / πεδίων μιας οντότητας
    /// </summary>
    public class ColumnMetadata
    {
        /// <summary>
        /// Όνομα πεδίου
        /// </summary>
        public string FieldName { get; private set; }
        /// <summary>
        /// Τυπος πεδίου
        /// </summary>
        public Type FieldType { get; private set; }
        /// <summary>
        /// Όνομα στήλης στον πίνακα
        /// </summary>
        public string ColumnName { get; private set; }
        /// <summary>
        /// Τυπος στήλης
        /// </summary>
        public Type ColumnType { get; private set; }
        /// <summary>
        /// Είναι μοναδικό αναγνωριστικό / πρωτεύον κλειδί;
        /// </summary>
        public bool IsIdentifier { get; private set; } = false;
        /// <summary>
        /// Οι τιμές της στήλης πρέπει να είναι μοναδικές;
        /// </summary>
        public bool IsUnique { get; private set; } = false;
        /// <summary>
        /// Η στήλη μπορεί να περιέχει NULL τιμες;
        /// </summary>
        public bool IsNullable { get; private set; } = true;
        /// <summary>
        /// Είναι παραμετρικό πεδίο;
        /// </summary>
        public bool IsCustomColumn { get; private set; } = false;
        /// <summary>
        /// Όνομα πίνακα παραμετρικών πεδίων
        /// </summary>
        public string CustomFieldTable { get; private set; }
        /// <summary>
        /// ID παραμετρικού πεδίου
        /// </summary>
        public int CustomFieldId{ get; private set; }
        /// <summary>
        /// Όνομα πεδίου το οποίο χρησιμοποιούμε ως αναφορά στον πατρικό πίνακα της οντότητας
        /// </summary>
        /// <remarks>Το σύστημα έχει έναν ιδιαίτερο τρόπο να αποθηκεύει έξτρα πληροφορίες. Μας δίνει την δυνατότητα
        /// να δημιουργήσουμε «παραμετρικά» πεδία. Επί της ουσίας, δημιουργείται ένας extra πίνακας στον οποίο έχουμε μια
        /// Many To One συσχετίσεις. Η πρώτη είναι με το παραμετρικό πεδίο - τα παραμετρικά σώζονται σε άλλο πίνακα - 
        /// και η δεύτερη με την συσχετισμένη οντότητα. Για να ξέρουμε με ποιο πεδίο θα πρέπει να συσχετίσουμε αρα
        /// και τν να ψάξουμε / γ΄ραψουμε, θα πρέπει να ξέρουμε που "Δενει" </remarks>
        public string CustomFieldReference { get; private set; }
        /// <summary>
        /// Είναι συσχέτιση;
        /// </summary>
        public bool IsRelationship { get; private set; }
        /// <summary>
        /// Τύπος συσχέτισης
        /// </summary>
        public RelationshipType RelationshipType { get; private set; }
        /// <summary>
        /// Συσχετιζόμενη οντότητα
        /// </summary>
        public Type TargetEntity { get; private set; }
        /// <summary>
        /// Αναφερόμενη στήλη στην συσχετισμένη οντότητα
        /// </summary>
        public string RelationshipReferenceColumn { get; private set; }
        /// <summary>
        /// FieldInfo για χρήση Reflection
        /// </summary>
        public FieldInfo FieldInfo { get; private set; }
        /// <summary>
        /// Είναι Guid?
        /// </summary>
        public bool IsRowGuid { get; private set; } = false;
        /// <summary>
        /// Δημιουργεί ένα νέο αντικείμενο μεταδεδομένων στήλης / πεδίου οντότητας
        /// </summary>
        /// <param name="FieldInfo"></param>
        public ColumnMetadata(FieldInfo FieldInfo)
        {
            this.FieldInfo = FieldInfo ?? throw new ArgumentNullException(nameof(FieldInfo));

            this.FieldInfo = FieldInfo;
            this.FieldName = FieldInfo.Name;
            this.FieldType = FieldInfo.FieldType;

            //Get Attributes
            ColumnAttribute column = this.FieldInfo.GetCustomAttribute<ColumnAttribute>();
            CustomColumnAttribute customColumn = this.FieldInfo.GetCustomAttribute<CustomColumnAttribute>();
            
            ManyToOneAttribute manyToOne = this.FieldInfo.GetCustomAttribute<ManyToOneAttribute>();
            OneToManyAttribute oneToMany = this.FieldInfo.GetCustomAttribute<OneToManyAttribute>();

            if (column == null && customColumn == null && oneToMany == null)
            {
                throw new MetadataException("This field does not contain any Column information");
            }

            this.IsIdentifier = (this.FieldInfo.GetCustomAttribute<IdentityAttribute>() != null);

            if (column != null)
            {
                this.ColumnName = column.ColumnName;
                this.ColumnType = GetType(column.ColumnType);
                this.IsNullable = column.Nullable;
                this.IsUnique = column.Unique;
            } 
            if (customColumn != null)
            {
                this.IsCustomColumn = true;
                //This is hardcoded since our database names the column that stores the value "CustomFieldValue"
                this.ColumnName = "CustomFieldValue";
                this.ColumnType = GetType(customColumn.ColumnType);
                //Default values
                this.IsNullable = true;
                this.IsUnique = false;

                this.CustomFieldTable = customColumn.CustomTableName;
                this.CustomFieldId = customColumn.CustomFieldId;
                this.CustomFieldReference = customColumn.IdentifierColumn;
            }
            if (manyToOne != null)
            {
                this.IsRelationship = true;
                this.TargetEntity = manyToOne.TargetEntity;
                this.RelationshipReferenceColumn = manyToOne.IdentifierColumn;
                this.RelationshipType = RelationshipType.ManyToOne;
            }
            if (oneToMany != null)
            {
                this.IsRelationship = true;
                this.TargetEntity = oneToMany.TargetEntity;
                this.RelationshipReferenceColumn = oneToMany.IdentifierColumn;
                this.RelationshipType = RelationshipType.OneToMany;
            }
            //ADDED Row Guid
            this.IsRowGuid = (this.FieldInfo.GetCustomAttribute<GuidAttribute>() != null);
            
        }
        private static Type GetType(ColumnType columnType)
        {
            switch(columnType)
            {
                case Attributes.ColumnType.Binary:
                    return typeof(SqlBinary);
                case Attributes.ColumnType.Boolean:
                case Attributes.ColumnType.BOOLEAN:
                    return typeof(SqlBoolean);
                case Attributes.ColumnType.Byte:
                    return typeof(SqlByte);
                case Attributes.ColumnType.Bytes:
                    return typeof(SqlBytes);
                case Attributes.ColumnType.Chars:
                    return typeof(SqlChars);
                case Attributes.ColumnType.DATE:
                case Attributes.ColumnType.DateTime:
                case Attributes.ColumnType.DATETIME:
                case Attributes.ColumnType.TIME:
                    return typeof(SqlDateTime);
                case Attributes.ColumnType.Decimal:
                    return typeof(SqlDecimal);
                case Attributes.ColumnType.Double:
                case Attributes.ColumnType.DOUBLE:
                    return typeof(SqlDouble);
                case Attributes.ColumnType.Guid:
                    return typeof(SqlGuid);
                case Attributes.ColumnType.Int16:
                    return typeof(SqlInt16);
                case Attributes.ColumnType.Int32:
                case Attributes.ColumnType.INTEGER:
                    return typeof(SqlInt32);
                case Attributes.ColumnType.Int64:
                    return typeof(SqlInt64);
                case Attributes.ColumnType.Money:
                    return typeof(SqlMoney);
                case Attributes.ColumnType.Single:
                    return typeof(SqlSingle);
                case Attributes.ColumnType.STRING:
                case Attributes.ColumnType.String:
                default:
                    return typeof(SqlString);
                case Attributes.ColumnType.Xml:
                    return typeof(SqlXml);
            }
        }

        internal string GetCustomColumnQuery()
        {
            return "IF (NOT EXISTS(Select CustomFieldValue FROM " + this.CustomFieldTable + " t" + Environment.NewLine +
                "WHERE t.CustomFieldId = @customFieldId and t." + this.CustomFieldReference + " = @identifier))" + Environment.NewLine +
                "BEGIN " + Environment.NewLine + 
                "INSERT INTO " + this.CustomFieldTable + " (" + this.CustomFieldReference + ", CustomFieldId, CustomFieldValue) VALUES " + Environment.NewLine +
                "(@identifier, @customFieldId, @fieldValue)" + Environment.NewLine +
                "END" + Environment.NewLine + 
                "ELSE" + Environment.NewLine +
                "BEGIN " + Environment.NewLine + 
                "UPDATE " + this.CustomFieldTable + " SET CustomFieldValue = @fieldValue" + Environment.NewLine +
                "WHERE " + this.CustomFieldReference + " = @identifier AND CustomFieldId = @customFieldId" + Environment.NewLine +
                "END";
        }
        internal Dictionary<string, object> GetCustomColumnParameters(object EntityObject) {

            if (EntityObject == null)
                throw new ArgumentNullException(nameof(EntityObject));

            ClassMetadata mainMeta = MetadataCache.Get(EntityObject.GetType());

            Dictionary<string, object> parameters = new Dictionary<string, object>();


            parameters.Add("@customFieldId", this.CustomFieldId);
            parameters.Add("@identifier", mainMeta.GetIdentifierField().GetValue(EntityObject));
            object temp = this.FieldInfo.GetValue(EntityObject);
            if (temp == null)
                temp = DBNull.Value;

            parameters.Add("@fieldValue", temp);

            return parameters;
        }
    }
    /// <summary>
    /// Enumeration τύπων συσχετίσεων
    /// </summary>
    public enum RelationshipType
    {
        ManyToOne,
        OneToMany
    }
}
