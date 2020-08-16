///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.Json;

using com.espertech.esper.common.@internal.@event.json.serde;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public class JsonDeserializerEventObjectArray : JsonDeserializerBase
    {
        private readonly Type _componentType;
        private readonly List<object> _events = new List<object>();
        private readonly JsonSerializationContext _factory;

        public JsonDeserializerEventObjectArray(
            JsonSerializationContext factory,
            Type componentType)
        {
            _factory = factory;
            _componentType = componentType;
        }

        public override object Deserialize(JsonElement element)
        {
            throw new NotImplementedException();
        }

        public override object GetResult() => 
            throw new NotImplementedException();

        public static object CollectionToTypedArray(
            ICollection<object> events,
            Type componentType)
        {
            var length = events.Count;
            var array = Array.CreateInstance(componentType, length);
            var enumerator = events.GetEnumerator();
            
            for (var ii = 0; ii < length && enumerator.MoveNext(); ii++) {
                array.SetValue(enumerator.Current, ii);
            }

            return array;
        }
    }
} // end of namespace