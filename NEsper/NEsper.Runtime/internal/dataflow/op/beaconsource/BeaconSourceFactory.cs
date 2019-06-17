///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.@internal.dataflow.op.beaconsource
{
    public class BeaconSourceFactory : DataFlowOperatorFactory
    {
        public ExprEvaluator[] PropertyEvaluators { get; set; }

        public EventBeanManufacturer Manufacturer { get; set; }

        public bool IsProduceEventBean { get; set; }

        public ExprEvaluator Iterations { get; set; }

        public ExprEvaluator InitialDelay { get; set; }

        public ExprEvaluator Interval { get; set; }

        public EventType OutputEventType { get; set; }

        public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
        {
            // no action
        }

        public DataFlowOperator Operator(DataFlowOpInitializeContext context)
        {
            var iterationsCount = DataFlowParameterResolution.ResolveNumber("iterations", Iterations, 0, context).AsLong();
            var initialDelaySec = DataFlowParameterResolution.ResolveNumber("initialDelay", InitialDelay, 0, context).AsDouble();
            var initialDelayMSec = (long) (initialDelaySec * 1000);
            var intervalSec = DataFlowParameterResolution.ResolveNumber("interval", Interval, 0, context).AsDouble();
            var intervalMSec = (long) (intervalSec * 1000);
            return new BeaconSourceOp(this, iterationsCount, initialDelayMSec, intervalMSec, context.AdditionalParameters);
        }
    }
} // end of namespace