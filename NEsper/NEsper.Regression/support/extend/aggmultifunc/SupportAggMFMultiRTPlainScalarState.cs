///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportAggMFMultiRTPlainScalarState : AggregationMultiFunctionState
    {
        private readonly SupportAggMFMultiRTPlainScalarStateFactory factory;

        public SupportAggMFMultiRTPlainScalarState(SupportAggMFMultiRTPlainScalarStateFactory factory)
        {
            this.factory = factory;
        }

        public object LastValue { get; private set; }

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            LastValue = factory.Param.Evaluate(eventsPerStream, true, exprEvaluatorContext);
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // ever semantics
        }

        public void Clear()
        {
            LastValue = null;
        }

        public int Size()
        {
            return LastValue == null ? 0 : 1;
        }
    }
} // end of namespace