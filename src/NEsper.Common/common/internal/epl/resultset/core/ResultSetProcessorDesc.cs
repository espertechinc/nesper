///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public class ResultSetProcessorDesc
    {
        private readonly ResultSetProcessorFlags flags;

        public ResultSetProcessorDesc(
            ResultSetProcessorFactoryForge resultSetProcessorFactoryForge,
            ResultSetProcessorFlags flags,
            ResultSetProcessorType resultSetProcessorType,
            SelectExprProcessorForge[] selectExprProcessorForges,
            EventType resultEventType,
            bool rollup,
            AggregationServiceForgeDesc aggregationServiceForgeDesc,
            OrderByProcessorFactoryForge orderByProcessorFactoryForge,
            SelectSubscriberDescriptor selectSubscriberDescriptor,
            IList<StmtClassForgeableFactory> additionalForgeables,
            FabricCharge fabricCharge)
        {
            ResultSetProcessorFactoryForge = resultSetProcessorFactoryForge;
            ResultSetProcessorType = resultSetProcessorType;
            SelectExprProcessorForges = selectExprProcessorForges;
            this.flags = flags;
            ResultEventType = resultEventType;
            IsRollup = rollup;
            AggregationServiceForgeDesc = aggregationServiceForgeDesc;
            OrderByProcessorFactoryForge = orderByProcessorFactoryForge;
            SelectSubscriberDescriptor = selectSubscriberDescriptor;
            AdditionalForgeables = additionalForgeables;
            FabricCharge = fabricCharge;
        }

        public ResultSetProcessorFactoryForge ResultSetProcessorFactoryForge { get; }

        public ResultSetProcessorType ResultSetProcessorType { get; }

        public SelectExprProcessorForge[] SelectExprProcessorForges { get; }

        public bool IsJoin => flags.IsJoin;

        public bool HasOutputLimit => flags.HasOutputLimit;

        public ResultSetProcessorOutputConditionType? OutputConditionType => flags.OutputConditionType;

        public bool HasOutputLimitSnapshot => flags.IsOutputLimitWSnapshot;

        public EventType ResultEventType { get; }

        public bool IsRollup { get; }

        public AggregationServiceForgeDesc AggregationServiceForgeDesc { get; }

        public OrderByProcessorFactoryForge OrderByProcessorFactoryForge { get; }

        public SelectSubscriberDescriptor SelectSubscriberDescriptor { get; }

        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }

        public FabricCharge FabricCharge { get; }
    }
} // end of namespace