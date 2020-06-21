///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue;

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.array
{
    public class JsonDelegateArrayEnum : JsonDelegateArrayBase
    {
        private readonly Type enumType;
        private readonly Method valueOf;

        public JsonDelegateArrayEnum(
            JsonHandlerDelegator baseHandler,
            JsonDelegateBase parent,
            Type enumType) : base(baseHandler, parent)
        {
            this.enumType = enumType;
            try {
                valueOf = enumType.GetMethod("valueOf", new[] {typeof(string)});
            }
            catch (NoSuchMethodException e) {
                throw new EPException("Failed to find valueOf method for " + enumType);
            }
        }

        public JsonDelegateArrayEnum(
            JsonHandlerDelegator baseHandler,
            JsonDelegateBase parent,
            Type enumType,
            Method valueOf) : base(baseHandler, parent)
        {
            this.enumType = enumType;
            this.valueOf = valueOf;
        }

        public override void EndOfArrayValue(string name)
        {
            collection.Add(JsonEndValueForgeEnum.JsonToEnum(StringValue, valueOf));
        }

        public override object GetResult()
        {
            return JsonDelegateEventObjectArray.CollectionToTypedArray(collection, enumType);
        }
    }
} // end of namespace