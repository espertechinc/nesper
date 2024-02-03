///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text.Json;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.core
{
    public class JsonDeserializerGenericObject : JsonDeserializerGenericBase
    {
        public override object Deserialize(JsonElement element)
        {
            return element.ValueKind switch {
                JsonValueKind.Object => element.ElementToDictionary(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.ElementToNumeric(),
                JsonValueKind.Array => element.ElementToArray(),
                _ => throw new IllegalStateException(
                    $"expected {nameof(JsonValueKind.Object)}, but received {element.ValueKind}")
            };
        }
    }
} // end of namespace