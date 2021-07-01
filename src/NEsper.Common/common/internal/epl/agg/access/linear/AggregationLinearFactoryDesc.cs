///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationLinearFactoryDesc
    {
        public AggregationLinearFactoryDesc(
            AggregationForgeFactory factory,
            EventType enumerationEventType,
            Type scalarCollectionType,
            int streamNum)
        {
            Factory = factory;
            EnumerationEventType = enumerationEventType;
            ScalarCollectionType = scalarCollectionType;
            StreamNum = streamNum;
        }

        public AggregationForgeFactory Factory { get; }

        public EventType EnumerationEventType { get; }

        public Type ScalarCollectionType { get; }

        public int StreamNum { get; }
    }
} // end of namespace