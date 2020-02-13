using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DBApi.Attributes;
namespace DBApi.Reflection
{
    /// <summary>
    /// Περιέχει τα μεταδεδομένα μιας ονότητας
    /// </summary>
    public class ClassMetadata
    {
        /// <summary>
        /// Όνομα Οντότητας / Κλάσης
        /// </summary>
        public string EntityName { get; private set; }
        /// <summary>
        /// Τύπος οντότητας
        /// </summary>
        public Type EntityType { get; private set; }
        /// <summary>
        /// Κλειδί λανθάνουσας μνήμης - για διαχείριση των μεταδεδομένων στην λανθάνουσα μνήμη
        /// </summary>
        public string CacheKey { get; private set; }
        /// <summary>
        /// Ο πίνακας που έχει συσχετιστεί με την οντόητα
        /// </summary>
        public string TableName { get; private set; }
        /// <summary>
        /// Το όνομα της στήλης του πίνακα που περιέχει το αναγνωριστικό / πρωτεύον κλειδί
        /// </summary>
        public string IdentifierColumn { get; private set; }
        /// <summary>
        /// Πίνακας στον οποίο αποθηκεύονται τα παραμετρικά πεδία
        /// </summary>
        public string CustomTable { get; private set; }
        /// <summary>
        /// Όνομα στηλης στον πίνακα παραμετρικών πεδίων, όπου γίνεται η αναφορά στην οντότητα
        /// </summary>
        public string CustomReferenceColumn { get; private set; }
        /// <summary>
        /// Στήλη η οποία περιέχει το Guid της γραμμής / οντότητας
        /// </summary>
        public string GuidColumn { get; private set; }
        /// <summary>
        /// Μεταδεδομένα πεδίων / στηλών της οντότητας
        /// </summary>
        public Dictionary<string, ColumnMetadata> Columns { get; private set; }
        /// <summary>
        /// Δημιουργεί ένα νέο αντικείμενο μεταδεδομένων οντότητας
        /// </summary>
        /// <param name="EntityType">Ο τύπος της κλάσης που έχει μαρκαριστεί ως οντότητα</param>
        public ClassMetadata(Type EntityType)
        {
            this.EntityType = EntityType ?? throw new ArgumentNullException(nameof(EntityType));
            if (!IsEntity(this.EntityType))
                throw MetadataException.InvalidEntity(this.EntityType);

            this.EntityName = this.EntityType.Name;
            this.CacheKey = GetCacheKey(this.EntityType);
            this.TableName = GetTableName(this.EntityType);
            this.Columns = GetColumns(this.EntityType);
            try
            {
                //Εαν η οντότητα δεν έχει καμία στηλή με το IdentifierAttribute, η παρακάτω
                //γραμμή θα σκάσει και θα μας πάρει στο λαιμό της. Εξ ού και το try-catch
                this.IdentifierColumn = this.Columns.Select(c => c.Value)
                    .Where(c => c.IsIdentifier == true)
                    .First().ColumnName;
            } catch (Exception ex)
            {
                throw MetadataException.MissingIdentifierAttribute(this.EntityName);
            }

            //Σε αντίθεση με παραπάνω, οι οντότητες δεν είναι υποχρεωμένες να έχουν 
            //παραμετρικά πεδία ή (πιο κάτω) στήλη Guid, οπότε εδώ αρκούν απλοί έλεγχοι
            //για την ύπαρξη παραμετρικών ή στήλης Guid
            if (this.HasCustomColumns())
            {
                this.CustomTable = this.Columns.Select(c => c.Value)
                    .Where(c => c.IsCustomColumn == true)
                    .FirstOrDefault().CustomFieldTable ?? string.Empty;
                this.CustomReferenceColumn = this.Columns.Select(c => c.Value)
                    .Where(c => c.IsCustomColumn == true)
                    .FirstOrDefault().CustomFieldReference ?? string.Empty;
            }

            if (HasGuidColumn())
            {
                this.GuidColumn = this.Columns.Select(c => c.Value)
                    .Where(c => c.IsRowGuid == true)
                    .FirstOrDefault().ColumnName ?? string.Empty;
            }
        }
        /// <summary>
        /// Ελέγχει εάν η οντότητα έχει στήλη Guid
        /// </summary>
        /// <returns>True αν υπάρχει στήλη με το GuidAttribute, αλλιώς false</returns>
        public bool HasGuidColumn()
        {
            return this.Columns.Select(c => c.Value)
                .Where(c => c.IsRowGuid)
                .Any();
        }
        /// <summary>
        /// Ελέγχει εάν υπάρχει μια συγκεκριμένη στηλη στην οντότητα
        /// </summary>
        /// <param name="columnName">Το όνομα της στήλης</param>
        /// <returns>True εάν υπάρχει η στήλη, αλλιώς false</returns>
        public bool ContainsColumn(string columnName)
        {
            return this.Columns.Select(c => c.Value)
                .Where(c => c.ColumnName == columnName)
                .Any();
        }
        /// <summary>
        /// Ελέγχει εάν υπάρχει ένα συγκεκριμένο πεδίο στην όντότητα
        /// </summary>
        /// <param name="fieldName">Το όνομα του πεδίου</param>
        /// <returns>True εάν υπάρχει το πεδίο, αλλιώς false</returns>
        public bool ContainsField(string fieldName)
        {
            return this.Columns.Select(c => c.Value)
                .Where(c => c.FieldName == fieldName)
                .Any();
        }
        /// <summary>
        /// Επιστρέφει τα μεταδεδομένα μιας στήλης
        /// </summary>
        /// <param name="columnName">Το όνομα της στήλης</param>
        /// <returns></returns>
        /// <exception cref="MetadataException">Πετάει exception εάν δεν υπάρχει η στήλη στην οντότητα</exception>
        public ColumnMetadata GetColumnMetadata(string columnName)
        {
            if (ContainsColumn(columnName))
                return this.Columns.Select(c => c.Value)
                    .Where(c => c.ColumnName == columnName)
                    .FirstOrDefault();
            throw MetadataException.NonExistentColumn(columnName, this.EntityName);
        }
        /// <summary>
        /// Επιστρέφει το <see cref="FieldInfo"/> μιας συγκεκριμένης στήλης, για χρήση (reflection)
        /// </summary>
        /// <param name="columnName">Το όνομα της στήλης</param>
        /// <returns></returns>
        /// <exception cref="MetadataException">Πετάει exception εάν η στήλη δεν υπάρχει στην οντότητα</exception>
        public FieldInfo GetColumnFieldInfo(string columnName)
        {
            return GetColumnMetadata(columnName).FieldInfo;
        }
        /// <summary>
        /// Επιστρέφει το <see cref="FieldInfo"/> της στήλης η οποία είναι το αναγνωριστικό / πρωτεύον κλειδί της οντότητας
        /// </summary>
        /// <returns></returns>
        public FieldInfo GetIdentifierField()
        {
            return GetColumnFieldInfo(this.IdentifierColumn);
        }
        /// <summary>
        /// Ελέγχει εαν η οντότητα έχει παραμετρικά πεδία
        /// </summary>
        /// <returns>True εαν η οντότητα έχει παραμετρικά, αλλιώς false</returns>
        public bool HasCustomColumns()
        {
            return this.Columns.Select(c => c.Value)
                .Where(c => c.IsCustomColumn == true)
                .Any();
        }
        /// <summary>
        /// Ελέγχει εάν η οντότητα έχει συσχετίσεις
        /// </summary>
        /// <returns>True εαν υπάρχουν συσχετίσεις, αλλιώς False</returns>
        public bool HasRelationships()
        {
            return this.Columns.Select(c => c.Value)
                .Where(c => c.IsRelationship == true)
                .Any();
        }
        /// <summary>
        /// Ελέγχει εαν η οντότητα έχει ένα παραμετρικό πεδίο
        /// </summary>
        /// <param name="customFieldId">To ID του παραμετρικού πεδίου</param>
        /// <returns>True εάν υπάρχει το παραμετρικό, αλλιώς false</returns>
        public bool HasCustomColumn(int customFieldId)
        {
            return this.Columns.Select(c => c.Value)
                .Where(c => c.IsCustomColumn && c.CustomFieldId == customFieldId)
                .Any();
        }
        /// <summary>
        /// Επιστρέφει τα μεταδεδομένα ενός παραμετρικού πεδίου της οντότητας
        /// </summary>
        /// <param name="customFieldId">Το ID του παραμετρικού πεδίου</param>
        /// <returns></returns>
        public ColumnMetadata GetCustomColumnMetadata(int customFieldId)
        {
            if (HasCustomColumn(customFieldId))
                return this.Columns.Select(c => c.Value)
                    .Where(c => c.IsCustomColumn && c.CustomFieldId == customFieldId)
                    .FirstOrDefault();
            throw MetadataException.NonExistentCustomColumn(customFieldId, this.EntityName);
        }
        /// <summary>
        /// Επιστρέφει το <see cref="FieldInfo"/> ενός παραμετρικού πεδίου της οντότητας
        /// </summary>
        /// <param name="customFieldId">Το ID του παραμετρικού πεδίου</param>
        /// <returns></returns>
        public FieldInfo GetCustomColumnFieldInfo(int customFieldId)
        {
            return GetCustomColumnMetadata(customFieldId).FieldInfo;
        }
        /// <summary>
        /// Επιστρέφει όλα τα πεδία της οντότητας για χρήση σε SELECTs
        /// </summary>
        /// <remarks>Κατά πάσα πιθανότητα θα χρειαστεί να φύγει</remarks>
        /// <returns></returns>
        public List<string> GetDatabaseFields()
        {
            var fieldColumn = Columns.Select(c => c.Value)
                .Where(c => (c.IsCustomColumn != true) || (c.IsRelationship == false) || (c.IsRelationship == true && c.RelationshipType == RelationshipType.ManyToOne))
                .ToList<ColumnMetadata>();
            return fieldColumn.Select(c => c.ColumnName)
                .ToList();
        }
        
        #region Static
        /// <summary>
        /// Δημιουργεί το κλειδί με το οποίο διαχειριζόμαστε τα μεταδεδομένα στην λανθάνουσα μνήμη
        /// </summary>
        /// <param name="entityType">Ο τύπος της κλάσης που έχει μαρκαριστεί ως οντότητα</param>
        /// <returns>Το κλειδί για την λανθάνουσα μνήμη</returns>
        public static string GetCacheKey(Type entityType) => $"{entityType.Namespace}_{entityType.Name}";
        /// <summary>
        /// Ελέγχει εάν η κλάση έχει μαρκαριστεί με το <see cref="EntityAttribute"/>
        /// </summary>
        /// <param name="entityType">Ο τύπος της κλάσης που έχει μαρκαριστεί ως οντότητα</param>
        /// <returns>True εάν η κλάση είναι όντότητα, αλλιώς false</returns>
        public static bool IsEntity(Type entityType) => (entityType.GetCustomAttribute<EntityAttribute>() != null;
        /// <summary>
        /// Επιστρέφει το όνομα του πίνακα που είναι συσχετισμένος με την όντότητα
        /// </summary>
        /// <param name="entityType">Ο τύπος της κλάσης που έχει μαρκαριστεί ως οντότητα</param>
        /// <returns></returns>
        /// <exception cref="MetadataException">Πετάει εξαίρεση σε περίπτωση που η οντότητα δεν έχει μαρκαριστεί με το <see cref="TableAttribute"/></exception>
        public static string GetTableName(Type entityType)
        {
            TableAttribute table = entityType.GetCustomAttribute<TableAttribute>();
            if (table == null)
                throw MetadataException.MissingTableAttribute(entityType);
            return table.TableName;
        }
        /// <summary>
        /// Φέρνει όλα τα πεδία της κλάσης μέσω reflection
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private static FieldInfo[] GetFields(Type entityType)
        {
            return entityType.GetFields(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.FlattenHierarchy
                );
        }
        /// <summary>
        /// Επιστρέφει όλα τα μεταδεδομένα των στηλών / πεδίων της οντότητας
        /// </summary>
        /// <param name="entityType">Ο τύπος της κλάσης που έχει μαρκαριστεί ως οντότητα</param>
        /// <returns></returns>
        private static Dictionary<string, ColumnMetadata> GetColumns(Type entityType)
        {
            Dictionary<string, ColumnMetadata> metadataDict = new Dictionary<string, ColumnMetadata>();
            foreach (var field in GetFields(entityType))
            {
                try
                {
                    ColumnMetadata meta = new ColumnMetadata(field);
                    //Αποθηκεύουμε το fieldName που είναι Unique
                    //Μην το αλλάξεις και βάλεις το ColumnName
                    //Σκάει το όλο σύστημα (καθότι υπάρχουν non-unique column names
                    //παραμετρικά πεδία λέγονται...)
                    metadataDict.Add(meta.FieldName, meta);
                }
                catch
                {
                    continue;
                }
            }
            return metadataDict;
        }

        internal static Dictionary<string, object> GetParameterDictionary(object EntityObject)
        {
            ClassMetadata metadata = new ClassMetadata(EntityObject.GetType());
            Dictionary<string, object> param = new Dictionary<string, object>();

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

                if (column.IsRelationship && column.RelationshipType == RelationshipType.ManyToOne)
                {
                    var relatedObject = column.FieldInfo.GetValue(EntityObject);
                    if (relatedObject != null)
                    {
                        //Speed Up Stuff
                        var objectMeta = MetadataCache.Get(relatedObject.GetType());

                        var relationshipIdentifier = objectMeta.GetIdentifierField().GetValue(relatedObject);
                        //ΑΤΤΝ: This fixes #30
                        var relationshipValue = objectMeta.Columns.Select(c => c.Value)
                            .Where(c => c.ColumnName == column.RelationshipReferenceColumn)
                            .FirstOrDefault()
                            .FieldInfo
                            .GetValue(relatedObject);

                        param.Add("@" + column.ColumnName, relationshipValue);
                    }
                    else
                    {
                        param.Add("@" + column.ColumnName, DBNull.Value);
                    }

                }
                else
                {
                    param.Add("@" + column.ColumnName, column.FieldInfo.GetValue(EntityObject));
                }
            }

            return param;
        }
        #endregion
    }
}
