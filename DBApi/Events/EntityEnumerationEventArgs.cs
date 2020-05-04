using System;
using System.Collections.Generic;
using System.Text;

namespace DBApi.Events
{
    public class EntityEnumerationEventArgs : EventArgs
    {
        public long EntityCount { get; }
        public Type EntityType { get; }

        public EntityEnumerationEventArgs(long entityCount, Type entityType)
        {
            EntityCount = entityCount;
            EntityType = entityType;
        }
    }

    public class EntityLoadedEventArgs : EventArgs
    {
        private readonly Type entityType;
        private readonly object identifier;

        public EntityLoadedEventArgs(Type entityType, object identifier)
        {
            this.entityType = entityType;
            this.identifier = identifier;
        }
    }
}
