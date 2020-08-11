///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue;
using com.espertech.esper.common.@internal.@event.json.write;

namespace com.espertech.esper.common.@internal.@event.json.parser.forge
{
    public class JsonForgeDesc
    {
        public JsonForgeDesc(
            string fieldName,
            JsonDelegateForge optionalStartObjectForge,
            JsonDelegateForge optionalStartArrayForge,
            JsonEndValueForge endValueForge,
            JsonWriteForge writeForge)
        {
            OptionalStartObjectForge = optionalStartObjectForge;
            OptionalStartArrayForge = optionalStartArrayForge;
            EndValueForge = endValueForge;
            WriteForge = writeForge;
            if (endValueForge == null || writeForge == null) {
                throw new ArgumentException("Unexpected null forge for end-value or write forge for field '" + fieldName + "'");
            }
        }

        public JsonDelegateForge OptionalStartObjectForge { get; }

        public JsonEndValueForge EndValueForge { get; }

        public JsonEndValueForge DeserializeForge { get; }

        public JsonDelegateForge OptionalStartArrayForge { get; }

        public JsonWriteForge WriteForge { get; }
    }
} // end of namespace