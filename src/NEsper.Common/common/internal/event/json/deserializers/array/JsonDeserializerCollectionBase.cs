///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using com.espertech.esper.common.@internal.@event.json.deserializers.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.array
{
    public abstract class JsonDeserializerCollectionBase<T> : JsonDeserializerBase
    {
        private readonly Func<JsonElement, object> _itemDeserializer;
        private IList<T> _result = EmptyList<T>.Instance;

        public JsonDeserializerCollectionBase(Func<JsonElement, object> itemDeserializer) : base()
        {
            _itemDeserializer = itemDeserializer;
        }

        public override object Deserialize(JsonElement element)
        {
            _result = element.ElementToArray(_itemDeserializer)
                .Cast<T>()
                .ToList();

            return _result;
        }
    }
} // end of namespace