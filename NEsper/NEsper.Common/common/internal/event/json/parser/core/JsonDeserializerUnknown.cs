///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text.Json;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public class JsonDeserializerUnknown : JsonDeserializerBase
    {
        public JsonDeserializerUnknown()
        {
        }

        public override object GetResult() => null;

        public override object Deserialize(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Undefined) {
                throw new IllegalStateException($"expected {nameof(JsonValueKind.Undefined)}, but received {element.ValueKind}");
            }

            return null;
        }
    }
} // end of namespace