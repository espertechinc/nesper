///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class JsonDeserializerCollection : JsonDeserializerBase
    {
        private readonly JsonDeserializerBase _itemDeserializer;
        private IList<object> _result = null;

        public JsonDeserializerCollection(JsonDeserializerBase itemDeserializer)
        {
            _itemDeserializer = itemDeserializer;
        }

        public override object Deserialize(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Array) {
                throw new IllegalStateException($"expected {nameof(JsonValueKind.Array)}, but received {element.ValueKind}");
            }

            _result = JsonElementExtensions.ElementToArray(element, _itemDeserializer.Deserialize);
            return _result;
        }
    }
} // end of namespace