///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.parser.delegates.array;

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.array2dim
{
    public class JsonDeserializerArray2DimLongPrimitive : JsonDeserializerArray2DimBase
    {
        public JsonDeserializerArray2DimLongPrimitive(
            JsonHandlerDelegator BaseHandler,
            JsonDeserializerBase parent) : base(parent)
        {
        }

        public override JsonDeserializerBase StartArrayInner()
        {
            return new JsonDeserializerArrayLongPrimitive(BaseHandler, this);
        }

        public override object GetResult()
        {
            return collection.Cast<long[]>().ToArray();
        }
    }
} // end of namespace