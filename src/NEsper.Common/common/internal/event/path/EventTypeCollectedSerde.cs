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
        private readonly EventTypeMetadata metadata;
        private readonly DataInputOutputSerde underlyingSerde;
        private readonly Type underlying;

        public EventTypeCollectedSerde(
            EventTypeMetadata metadata,
            DataInputOutputSerde underlyingSerde,
            Type underlying)
        {
            this.metadata = metadata;
            this.underlyingSerde = underlyingSerde;
            this.underlying = underlying;
        }

        public EventTypeMetadata Metadata => metadata;

        public DataInputOutputSerde UnderlyingSerde => underlyingSerde;

        public Type Underlying => underlying;
    }
} // end of namespace