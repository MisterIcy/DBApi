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
        public string EntityName { get; }
        /// <summary>
        /// Τύπος οντότητας
        /// </summary>
        public Type EntityType { get; }
        /// <summary>
        /// Κλειδί λανθάνουσας μνήμης - για διαχείριση των μεταδεδομένων στην λανθάνουσα μνήμη
        /// </summary>
        public string CacheKey { get; }
        /// <summary>
        /// Ο πίνακας που έχει συσχετιστεί με την οντόητα
        /// </summary>
        public string TableName { get; }
        /// <summary>
        /// Το όνομα της στήλης του πίνακα που περιέχει το αναγνωριστικό / πρωτεύον κλειδί
        /// </summary>
        public string IdentifierColumn { get; }
        /// <summary>
        /// Πίνακας στον οποίο αποθηκεύονται τα παραμετρικά πεδία
        /// </summary>
        public string CustomTable { get; }
        /// <summary>
        /// Όνομα στηλης στον πίνακα παραμετρικών πεδίων, όπου γίνεται η αναφορά στην οντότητα
        /// </summary>
        public string CustomReferenceColumn { get; }
        /// <summary>
        /// Στήλη η οποία περιέχει το Guid της γραμμής / οντότητας
        /// </summary>
        public string GuidColumn { get; }
        /// <summary>
        /// Cache Duration
        /// </summary>
        public long CacheDuration { get; } = 3600;
        /// <summary>
        /// NO cache
        /// </summary>
        public bool NoCache { get; }
        /// <summary>
        /// Μεταδεδομένα πεδίων / στηλών της οντότητας
        /// </summary>
        public Dictionary<string, ColumnMetadata> Columns { get; }
        
        /// <summary>
        /// Gets or sets a value indicating that this entity type supports optimistic locking
        /// </summary>
        public bool SupportsOptimisticLocking { get; }
        
        public string VersionColumn { get; }
        
        /// <summary>
        /// Δημιουργεί ένα νέο αντικείμενο μεταδεδομένων οντότητας
        /// </summary>
        /// <param name="EntityType">Ο τύπος της κλάσης που έχει μαρκαριστεί ως οντότητα</param>
        public ClassMetadata(Type EntityType)
        {
            this.EntityType = EntityType ?? throw new ArgumentNullException(nameof(EntityType));
            if (!IsEntity(this.EntityType))
                throw MetadataException.InvalidEntity(this.EntityType);

            EntityName = this.EntityType.Name;
            CacheKey = GetCacheKey(this.EntityType);
            TableName = GetTableName(this.EntityType);
            Columns = GetColumns(this.EntityType);
            try
            {
                //Εαν η οντότητα δεν έχει καμία στηλή με το IdentifierAttribute, η παρακάτω
                //γραμμή θα σκάσει και θα μας πάρει στο λαιμό της. Εξ ού και το try-catch
                IdentifierColumn = Columns
                    .Select(c => c.Value)
                    .First(c => c.IsIdentifier).ColumnName;
            } catch (Exception)
            {
                throw MetadataException.MissingIdentifierAttribute(EntityName);
            }

            //Σε αντίθεση με παραπάνω, οι οντότητες δεν είναι υποχρεωμένες να έχουν 
            //παραμετρικά πεδία ή (πιο κάτω) στήλη Guid, οπότε εδώ αρκούν απλοί έλεγχοι
            //για την ύπαρξη παραμετρικών ή στήλης Guid
            if (HasCustomColumns())
            {
                CustomTable = Columns
                    .Select(c => c.Value)
                    .FirstOrDefault(c => c.IsCustomColumn)
                    ?.CustomFieldTable ?? string.Empty;
                CustomReferenceColumn = Columns
                    .Select(c => c.Value)
                    .FirstOrDefault(c => c.IsCustomColumn)
                    ?.CustomFieldReference ?? string.Empty;
            }

            if (HasGuidColumn())
            {
                GuidColumn = Columns
                    .Select(c => c.Value)
                    .FirstOrDefault(c => c.IsRowGuid)
                    ?.ColumnName ?? string.Empty;
            }

            CacheControlAttribute cacheAttr = this.EntityType.GetCustomAttribute<CacheControlAttribute>();
            if (cacheAttr != null)
            {
                CacheDuration = cacheAttr.Duration;
                NoCache = cacheAttr.NoCache;
            }

            SupportsOptimisticLocking = Columns
                .Select(c => c.Value)
                .Any(c => c.IsVersion);
            if (SupportsOptimisticLocking)
            {
                VersionColumn = Columns
                    .Select(c => c.Value)
                    .FirstOrDefault(c => c.IsVersion)
                    ?.ColumnName ?? string.Empty;
            }
        }
        /// <summary>
        /// Ελέγχει εάν η οντότητα έχει στήλη Guid
        /// </summary>
        /// <returns>True αν υπάρχει στήλη με το GuidAttribute, αλλιώς false</returns>
        public bool HasGuidColumn()
        {
            return Columns
                .Select(c => c.Value)
                .Any(c => c.IsRowGuid);
        }
        /// <summary>
        /// Ελέγχει εάν υπάρχει μια συγκεκριμένη στηλη στην οντότητα
        /// </summary>
        /// <param name="columnName">Το όνομα της στήλης</param>
        /// <returns>True εάν υπάρχει η στήλη, αλλιώς false</returns>
        public bool ContainsColumn(string columnName)
        {
            return Columns
                .Select(c => c.Value)
                .Any(c => c.ColumnName == columnName);
        }
        /// <summary>
        /// Ελέγχει εάν υπάρχει ένα συγκεκριμένο πεδίο στην όντότητα
        /// </summary>
        /// <param name="fieldName">Το όνομα του πεδίου</param>
        /// <returns>True εάν υπάρχει το πεδίο, αλλιώς false</returns>
        public bool ContainsField(string fieldName)
        {
            return Columns
                .Select(c => c.Value)
                .Any(c => c.FieldName == fieldName);
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
                return Columns
                    .Select(c => c.Value)
                    .FirstOrDefault(c => c.ColumnName == columnName);
            throw MetadataException.NonExistentColumn(columnName, EntityName);
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
            return GetColumnFieldInfo(IdentifierColumn);
        }
        /// <summary>
        /// Ελέγχει εαν η οντότητα έχει παραμετρικά πεδία
        /// </summary>
        /// <returns>True εαν η οντότητα έχει παραμετρικά, αλλιώς false</returns>
        public bool HasCustomColumns()
        {
            return Columns
                .Select(c => c.Value)
                .Any(c => c.IsCustomColumn);
        }
        /// <summary>
        /// Ελέγχει εάν η οντότητα έχει συσχετίσεις
        /// </summary>
        /// <returns>True εαν υπάρχουν συσχετίσεις, αλλιώς False</returns>
        public bool HasRelationships()
        {
            return Columns
                .Select(c => c.Value)
                .Any(c => c.IsRelationship);
        }
        /// <summary>
        /// Ελέγχει εαν η οντότητα έχει ένα παραμετρικό πεδίο
        /// </summary>
        /// <param name="customFieldId">To ID του παραμετρικού πεδίου</param>
        /// <returns>True εάν υπάρχει το παραμετρικό, αλλιώς false</returns>
        public bool HasCustomColumn(int customFieldId)
        {
            return Columns
                .Select(c => c.Value)
                .Any(c => c.IsCustomColumn && c.CustomFieldId == customFieldId);
        }
        /// <summary>
        /// Επιστρέφει τα μεταδεδομένα ενός παραμετρικού πεδίου της οντότητας
        /// </summary>
        /// <param name="customFieldId">Το ID του παραμετρικού πεδίου</param>
        /// <returns></returns>
        public ColumnMetadata GetCustomColumnMetadata(int customFieldId)
        {
            if (HasCustomColumn(customFieldId))
                return Columns
                    .Select(c => c.Value)
                    .FirstOrDefault(c => c.IsCustomColumn && c.CustomFieldId == customFieldId);
            throw MetadataException.NonExistentCustomColumn(customFieldId, EntityName);
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
                .Where(c => (c.IsCustomColumn != true) || (c.IsRelationship == false) || (c.IsRelationship && c.RelationshipType == RelationshipType.ManyToOne))
                .ToList();
            return fieldColumn.Select(c => c.ColumnName)
                .ToList();
        }

        public FieldInfo GetVersionField()
        {
            return GetColumnFieldInfo(VersionColumn);
        }

        public object GetVersion(object entity)
        {
            return GetVersionField().GetValue(entity);
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
        public static bool IsEntity(Type entityType) => (entityType.GetCustomAttribute<EntityAttribute>() != null);
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
                    // ignored
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
                        var relationshipValue = objectMeta.Columns
                            .Select(c => c.Value)
                            .FirstOrDefault(c => c.ColumnName == column.RelationshipReferenceColumn)
                            ?.FieldInfo
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
