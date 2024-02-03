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

using com.espertech.esper.common.@internal.@event.json.deserializers.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.array2dim
{
    public class JsonDeserializerArray2DimEnum : JsonDeserializerBase
    {
        private readonly Type _enumType;
        private readonly Type _enumTypeArray;
        private Array _result = null;

        public JsonDeserializerArray2DimEnum(Type enumType)
        {
            _enumType = enumType;
            _enumTypeArray = TypeHelper.GetArrayType(enumType);
        }

        public Type EnumType => _enumType;

        public Type EnumTypeArray => _enumTypeArray;

        private Array ToEnumArray(IList<object> baseList)
        {
            var asArray = Arrays.CreateInstanceChecked(_enumType, baseList.Count);
            for (var ii = 0; ii < baseList.Count; ii++) {
                asArray.SetValue(baseList[ii], ii);
            }

            return asArray;
        }

        private Array To2DArray(IList<Array> baseList)
        {
            var arrayType = _enumType.MakeArrayType();
            var asArray = Arrays.CreateInstanceChecked(arrayType, baseList.Count);
            for (var ii = 0; ii < baseList.Count; ii++) {
                asArray.SetValue(baseList[ii], ii);
            }

            return asArray;
        }

        protected IList<Array> DeserializeToList(JsonElement element)
        {
            return
                element.ElementToArray(
                    e => ToEnumArray(
                        element.ElementToArray(
                            _ => Enum.Parse(_enumType, _.GetString()))));
        }

        public override object Deserialize(JsonElement element)
        {
            var baseList = DeserializeToList(element);
            _result = To2DArray(baseList);
            return _result;
        }
    }
} // end of namespace