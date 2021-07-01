///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.@event.json.deserializers.array
{
    public class JsonDeserializerCollectionPrimitiveInt64 : JsonDeserializerCollectionBase<long>
    {
        public JsonDeserializerCollectionPrimitiveInt64()
            : base(_ => _.GetSmartInt64())
        {
        }
    }
} // end of namespace
