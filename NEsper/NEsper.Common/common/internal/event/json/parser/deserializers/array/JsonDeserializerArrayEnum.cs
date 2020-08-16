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

namespace com.espertech.esper.common.@internal.@event.json.parser.deserializers.array
{
    public class JsonDeserializerArrayEnum : JsonDeserializerBase
    {
        private readonly Type _enumType;
        private Array _result = null;

        public JsonDeserializerArrayEnum(Type enumType) : base()
        {
            _enumType = enumType;
        }

        public Type EnumType => _enumType;

        public override object GetResult() => _result;

        protected IList<object> DeserializeToList(JsonElement element)
        {
            return JsonElementExtensions
                .ElementToArray(element, e => {
                    var baseValue = e.GetString();
                    return Enum.Parse(_enumType, baseValue);
                    // throw new FormatException("unexpected value for enum \"" + baseValue + "\"");
                });
        }
        
        public override object Deserialize(JsonElement element)
        {
            var baseArray = DeserializeToList(element);
            var trueArray = Array.CreateInstance(_enumType, baseArray.Count);
            for (int ii = 0; ii < baseArray.Count; ii++) {
                trueArray.SetValue(baseArray[ii], ii);
            }

            _result = trueArray;
            return _result;
        }
    }
} // end of namespace