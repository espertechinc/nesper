///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.@event.json.deserializers.array
{
    public class JsonDeserializerArrayPrimitiveDecimal : JsonDeserializerArrayBase<decimal>
    {
        public JsonDeserializerArrayPrimitiveDecimal()
            : base(_ => _.GetSmartDecimal())
        {
        }
    }
} // end of namespace