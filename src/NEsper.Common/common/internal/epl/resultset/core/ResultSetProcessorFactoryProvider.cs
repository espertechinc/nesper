///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.resultset.order;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public interface ResultSetProcessorFactoryProvider
    {
        ResultSetProcessorFactory ResultSetProcessorFactory { get; }

        AggregationServiceFactory AggregationServiceFactory { get; }

        OrderByProcessorFactory OrderByProcessorFactory { get; }

        ResultSetProcessorType ResultSetProcessorType { get; }

        EventType ResultEventType { get; }
    }
} // end of namespace