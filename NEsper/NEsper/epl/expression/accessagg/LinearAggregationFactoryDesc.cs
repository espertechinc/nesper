///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.epl.expression.accessagg
{
    public class LinearAggregationFactoryDesc
    {
        public LinearAggregationFactoryDesc(AggregationMethodFactory factory, EventType enumerationEventType, Type scalarCollectionType)
        {
            Factory = factory;
            EnumerationEventType = enumerationEventType;
            ScalarCollectionType = scalarCollectionType;
        }

        public AggregationMethodFactory Factory { get; private set; }

        public EventType EnumerationEventType { get; private set; }

        public Type ScalarCollectionType { get; private set; }
    }
}
