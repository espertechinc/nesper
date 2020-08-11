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
    public class JsonDeserializerCollectionBoxedDateTime : JsonDeserializerCollectionBase<DateTime?>
    {
        public JsonDeserializerCollectionBoxedDateTime(JsonDeserializerBase parent)
            : base(parent, _ => _.GetBoxedDateTime())
        {
        }
    }
} // end of namespace
