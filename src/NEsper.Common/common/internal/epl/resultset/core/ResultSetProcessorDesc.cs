///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.resultset.order;
using com.espertech.esper.common.@internal.epl.resultset.select.core;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public class ResultSetProcessorDesc
    {
        public ResultSetProcessorDesc(
            ResultSetProcessorFactoryForge resultSetProcessorFactoryForge,
            ResultSetProcessorType resultSetProcessorType,
            SelectExprProcessorForge[] selectExprProcessorForges,
            bool join,
            bool hasOutputLimit,
            ResultSetProcessorOutputConditionType? outputConditionType,
            bool hasOutputLimitSnapshot,
            EventType resultEventType,
            bool rollup,
            AggregationServiceForgeDesc aggregationServiceForgeDesc,
            OrderByProcessorFactoryForge orderByProcessorFactoryForge,
            SelectSubscriberDescriptor selectSubscriberDescriptor,
            IList<StmtClassForgeableFactory> additionalForgeables)
        {
            ResultSetProcessorFactoryForge = resultSetProcessorFactoryForge;
            ResultSetProcessorType = resultSetProcessorType;
            SelectExprProcessorForges = selectExprProcessorForges;
            IsJoin = join;
            HasOutputLimit = hasOutputLimit;
            OutputConditionType = outputConditionType;
            HasOutputLimitSnapshot = hasOutputLimitSnapshot;
            ResultEventType = resultEventType;
            IsRollup = rollup;
            AggregationServiceForgeDesc = aggregationServiceForgeDesc;
            OrderByProcessorFactoryForge = orderByProcessorFactoryForge;
            SelectSubscriberDescriptor = selectSubscriberDescriptor;
            AdditionalForgeables = additionalForgeables;
        }

        public ResultSetProcessorFactoryForge ResultSetProcessorFactoryForge { get; }

        public ResultSetProcessorType ResultSetProcessorType { get; }

        public SelectExprProcessorForge[] SelectExprProcessorForges { get; }

        public bool IsJoin { get; }

        public bool HasOutputLimit { get; }

        public ResultSetProcessorOutputConditionType? OutputConditionType { get; }

        public bool HasOutputLimitSnapshot { get; }

        public EventType ResultEventType { get; }

        public bool IsRollup { get; }

        public AggregationServiceForgeDesc AggregationServiceForgeDesc { get; }

        public OrderByProcessorFactoryForge OrderByProcessorFactoryForge { get; }

        public SelectSubscriberDescriptor SelectSubscriberDescriptor { get; }
        
        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }
    }
} // end of namespace