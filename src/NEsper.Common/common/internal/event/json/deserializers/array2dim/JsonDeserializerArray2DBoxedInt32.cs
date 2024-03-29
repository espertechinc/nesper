///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.@event.json.deserializers.array;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.array2dim
{
    public class JsonDeserializerArray2DBoxedInt32 : JsonDeserializerArrayBase<int?[]>
    {
        public JsonDeserializerArray2DBoxedInt32()
            : base(_ => _.ElementToArray(v => v.GetBoxedInt32()))
        {
        }
    }
} // end of namespace