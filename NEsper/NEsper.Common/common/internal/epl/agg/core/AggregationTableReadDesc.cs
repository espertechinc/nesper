///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationTableReadDesc
    {
        private readonly AggregationTableAccessAggReaderForge reader;
        private readonly EventType eventTypeCollection;
        private readonly Type componentTypeCollection;
        private readonly EventType eventTypeSingle;

        public AggregationTableReadDesc(
            AggregationTableAccessAggReaderForge reader,
            EventType eventTypeCollection,
            Type componentTypeCollection,
            EventType eventTypeSingle)
        {
            this.reader = reader;
            this.eventTypeCollection = eventTypeCollection;
            this.componentTypeCollection = componentTypeCollection;
            this.eventTypeSingle = eventTypeSingle;
        }

        public AggregationTableAccessAggReaderForge Reader {
            get => reader;
        }

        public EventType EventTypeCollection {
            get => eventTypeCollection;
        }

        public Type ComponentTypeCollection {
            get => componentTypeCollection;
        }

        public EventType EventTypeSingle {
            get => eventTypeSingle;
        }
    }
} // end of namespace