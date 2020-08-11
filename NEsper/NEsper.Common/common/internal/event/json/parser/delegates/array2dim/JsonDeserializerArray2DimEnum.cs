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
using com.espertech.esper.common.@internal.@event.json.parser.delegates.array;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.array2dim
{
    public class JsonDeserializerArray2DimEnum : JsonDeserializerArray2DimBase
    {
        private readonly Type enumType;
        private readonly Type enumTypeArray;
        private readonly Method valueOf;

        public JsonDeserializerArray2DimEnum(
            JsonHandlerDelegator BaseHandler,
            JsonDeserializerBase parent,
            Type enumType) : base(parent)
        {
            this.enumType = enumType;
            enumTypeArray = TypeHelper.GetArrayType(enumType);
            try {
                valueOf = enumType.GetMethod("valueOf", new[] {typeof(string)});
            }
            catch (NoSuchMethodException e) {
                throw new EPException("Failed to find valueOf method for " + enumType);
            }
        }

        public override JsonDeserializerBase StartArrayInner()
        {
            return new JsonDeserializerArrayEnum(BaseHandler, this, enumType, valueOf);
        }

        public override object GetResult()
        {
            return JsonDeserializerEventObjectArray.CollectionToTypedArray(collection, enumTypeArray);
        }
    }
} // end of namespace