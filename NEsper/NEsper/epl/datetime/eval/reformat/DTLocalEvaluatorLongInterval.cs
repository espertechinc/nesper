///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.datetime.interval;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.datetime.eval.reformat
{
    internal class DTLocalEvaluatorLongInterval : DTLocalEvaluatorIntervalBase
    {
        internal DTLocalEvaluatorLongInterval(IntervalOp intervalOp)
            : base(intervalOp)
        {
        }

        public override object Evaluate(object target, EvaluateParams evaluateParams)
        {
            var time = (long)target;
            return IntervalOp.Evaluate(time, time, evaluateParams);
        }

        public override object Evaluate(object startTimestamp, object endTimestamp, EvaluateParams evaluateParams)
        {
            var startTime = (long)startTimestamp;
            var endTime = (long)endTimestamp;
            return IntervalOp.Evaluate(startTime, endTime, evaluateParams);
        }
    }
}
