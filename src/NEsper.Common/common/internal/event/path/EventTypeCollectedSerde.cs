///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.serde;

namespace com.espertech.esper.common.@internal.@event.path
{
    public class EventTypeCollectedSerde
    {
        public EventTypeCollectedSerde(
            EventTypeMetadata metadata,
            DataInputOutputSerde underlyingSerde,
            Type underlying)
        {
            Metadata = metadata;
            UnderlyingSerde = underlyingSerde;
            Underlying = underlying;
        }

        public EventTypeMetadata Metadata { get; }

        public DataInputOutputSerde UnderlyingSerde { get; }

        public Type Underlying { get; }
    }
} // end of namespace