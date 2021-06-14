///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Text.Json;

using com.espertech.esper.common.@internal.@event.json.deserializers.core;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.array
{
    public abstract class JsonDeserializerArrayBase<T> : JsonDeserializerBase
    {
        private readonly Func<JsonElement, object> _itemDeserializer;
        private T[] _result = new T[0];

        public JsonDeserializerArrayBase(Func<JsonElement, object> itemDeserializer) : base()
        {
            _itemDeserializer = itemDeserializer;
        }

        public override object Deserialize(JsonElement element)
        {
            _result = JsonElementExtensions.ElementToArray<object>(element, _itemDeserializer)
                .Cast<T>()
                .ToArray();
            
            return _result;
        }
    }
} // end of namespace