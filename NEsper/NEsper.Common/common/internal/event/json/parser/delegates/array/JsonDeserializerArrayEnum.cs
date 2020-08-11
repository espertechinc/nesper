///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue;

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.array
{
    public class JsonDeserializerArrayEnum : JsonDeserializerBase
    {
        private readonly Type _enumType;
        private Array _result = null;

        public JsonDeserializerArrayEnum(
            JsonDeserializerBase parent,
            Type enumType) : base(parent)
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
                    if (baseValue is string baseValueString) {
                        return Enum.Parse(_enumType, baseValueString);
                    }
                    else {
                        throw new FormatException("unexpected value for enum \"" + baseValue + "\"");
                    }
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