///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.array
{
    public abstract class JsonDeserializerCollectionBase<T> : JsonDeserializerBase
    {
        private readonly JsonDeserializer _itemDeserializer;
        private IList<T> _result = EmptyList<T>.Instance;

        public JsonDeserializerCollectionBase(
            JsonDeserializerBase parent,
            JsonDeserializer itemDeserializer) : base(parent)
        {
            _itemDeserializer = itemDeserializer;
        }

        public override object GetResult() => _result;

        public override object Deserialize(JsonElement element)
        {
            _result = JsonElementExtensions.ElementToArray(element, _itemDeserializer)
                .Cast<T>()
                .ToList();
            
            return _result;
        }
    }
} // end of namespace