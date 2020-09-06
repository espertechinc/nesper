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

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public class JsonDeserializerGenericObject : JsonDeserializerGenericBase
    {
        private IDictionary<string, object> _result;

        public override object GetResult()
        {
            return _result;
        }

        public override object Deserialize(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object) {
                throw new IllegalStateException($"expected {nameof(JsonValueKind.Object)}, but received {element.ValueKind}");
            }

            _result = JsonElementExtensions.ElementToDictionary(element);
            return _result;
        }
    }
} // end of namespace