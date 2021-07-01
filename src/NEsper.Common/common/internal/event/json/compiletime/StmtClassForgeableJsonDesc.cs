///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.forge;

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
    public class StmtClassForgeableJsonDesc
    {
        public StmtClassForgeableJsonDesc(
            IDictionary<string, object> propertiesThisType,
            IDictionary<string, JsonUnderlyingField> fieldDescriptorsInclSupertype,
            bool dynamic,
            int numFieldsSupertype,
            JsonEventType optionalSupertype,
            IDictionary<string, JsonForgeDesc> forges)
        {
            PropertiesThisType = propertiesThisType;
            FieldDescriptorsInclSupertype = fieldDescriptorsInclSupertype;
            IsDynamic = dynamic;
            NumFieldsSupertype = numFieldsSupertype;
            OptionalSupertype = optionalSupertype;
            Forges = forges;
        }

        public IDictionary<string, object> PropertiesThisType { get; }

        public IDictionary<string, JsonUnderlyingField> FieldDescriptorsInclSupertype { get; }

        public bool IsDynamic { get; }

        public int NumFieldsSupertype { get; }

        public JsonEventType OptionalSupertype { get; }

        public IDictionary<string, JsonForgeDesc> Forges { get; }
    }
} // end of namespace