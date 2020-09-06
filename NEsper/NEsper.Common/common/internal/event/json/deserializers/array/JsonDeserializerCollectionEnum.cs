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

using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.array
{
    public class JsonDeserializerCollectionEnum : JsonDeserializerArrayEnum
    {
        private object _typedResult;

        public JsonDeserializerCollectionEnum(
            JsonDeserializerBase parent,
            Type enumType) : base(enumType)
        {
        }

        public override object Deserialize(JsonElement element)
        {
            var genericList = DeserializeToList(element);
            var typedListType = typeof(List<>).MakeGenericType(EnumType);
            var typedListAdd = typedListType.GetMethod("Add", new Type[] {EnumType});
            var typedList = typedListType.GetDefaultConstructor().Invoke(null);

            foreach (var enumValue in genericList) {
                typedListAdd.Invoke(typedList, new object[] {enumValue});
            }

            _typedResult = typedList;
            return _typedResult;
        }
    }
} // end of namespace