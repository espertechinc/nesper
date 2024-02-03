///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.Json;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.core
{
    public class JsonDeserializerEventObjectArray : JsonDeserializerBase
    {
        private readonly Type _componentType;
        private readonly List<object> _events = new List<object>();

        public JsonDeserializerEventObjectArray(Type componentType)
        {
            _componentType = componentType;
        }

        public override object Deserialize(JsonElement element)
        {
            throw new NotImplementedException();
        }

        public static object CollectionToTypedArray(
            ICollection<object> events,
            Type componentType)
        {
            var length = events.Count;
            var array = Arrays.CreateInstanceChecked(componentType, length);
            var enumerator = events.GetEnumerator();

            for (var ii = 0; ii < length && enumerator.MoveNext(); ii++) {
                array.SetValue(enumerator.Current, ii);
            }

            return array;
        }
    }
} // end of namespace