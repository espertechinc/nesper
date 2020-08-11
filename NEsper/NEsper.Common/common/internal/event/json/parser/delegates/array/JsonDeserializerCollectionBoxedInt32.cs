///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.common.@internal.@event.json.parser.core;

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.array
{
    public class JsonDeserializerCollectionBoxedInt32 : JsonDeserializerCollectionBase<int?>
    {
        public JsonDeserializerCollectionBoxedInt32(JsonDeserializerBase parent)
            : base(parent, _ => _.GetBoxedInt32())
        {
        }
    }
} // end of namespace
