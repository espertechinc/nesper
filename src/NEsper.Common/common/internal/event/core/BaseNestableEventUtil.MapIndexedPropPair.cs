using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.@event.core
{
    public partial class BaseNestableEventUtil
    {
        public class MapIndexedPropPair
        {
            public MapIndexedPropPair(
                ISet<string> mapProperties,
                ISet<string> arrayProperties)
            {
                MapProperties = mapProperties;
                ArrayProperties = arrayProperties;
            }

            public ISet<string> MapProperties { get; }

            public ISet<string> ArrayProperties { get; }
        }
    }
}