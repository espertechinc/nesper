///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.rettype;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableMetadataColumnAggregation : TableMetadataColumn
    {
        public TableMetadataColumnAggregation(string columnName, AggregationMethodFactory factory, int methodOffset, AggregationAccessorSlotPair accessAccessorSlotPair, EPType optionalEnumerationType, EventType optionalEventType)
            : base(columnName, false)
        {
            Factory = factory;
            MethodOffset = methodOffset;
            AccessAccessorSlotPair = accessAccessorSlotPair;
            OptionalEnumerationType = optionalEnumerationType;
            OptionalEventType = optionalEventType;
        }

        public AggregationMethodFactory Factory { get; private set; }

        public int MethodOffset { get; private set; }

        public AggregationAccessorSlotPair AccessAccessorSlotPair { get; private set; }

        public EPType OptionalEnumerationType { get; private set; }

        public EventType OptionalEventType { get; private set; }
    }
}
