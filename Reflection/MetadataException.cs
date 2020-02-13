using System;
using System.Runtime.Serialization;


namespace DBApi.Reflection
{
    public sealed class MetadataException : Exception
    {
        #region Default Constructors
        public MetadataException()
        {
        }

        public MetadataException(string message) : base(message)
        {
        }

        public MetadataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MetadataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        #endregion

        public static MetadataException InvalidEntity(Type entityType)
        {
            return new MetadataException($"Class `{entityType.Name}` is not a valid entity. Entity Attribute is missing");
        }

        internal static MetadataException MissingTableAttribute(Type entityType)
        {
            return new MetadataException($"Entity `{entityType.Name}` does not contain a Table attribute");
        }

        internal static MetadataException NonExistentColumn(string columnName, string entityName)
        {
            return new MetadataException($"Entity `{entityName}` does not contain a `{columnName}` column");
        }

        internal static MetadataException NonExistentCustomColumn(int customFieldId, string entityName)
        {
            return new MetadataException($"Entity `{entityName}` does not contain a custom column bound to custom field with id = `{customFieldId}`");
        }

        internal static Exception MissingIdentifierAttribute(string entityName)
        {
            return new MetadataException($"Entity `{entityName}` does not contain a column with IdentifierAttribute.");
        }
    }
}
