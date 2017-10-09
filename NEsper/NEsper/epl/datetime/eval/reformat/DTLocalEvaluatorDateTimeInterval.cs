///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.datetime.interval;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.datetime.eval.reformat
{
    internal class DTLocalEvaluatorDateTimeInterval : DTLocalEvaluatorIntervalBase
    {
        internal DTLocalEvaluatorDateTimeInterval(IntervalOp intervalOp)
            : base(intervalOp)
        {
        }

        public override object Evaluate(object target, EvaluateParams evaluateParams)
        {
            var time = ((DateTime)target).UtcMillis();
            return IntervalOp.Evaluate(time, time, evaluateParams);
        }

        public override object Evaluate(object startTimestamp, object endTimestamp, EvaluateParams evaluateParams)
        {
            var start = ((DateTime)startTimestamp).UtcMillis();
            var end = ((DateTime)endTimestamp).UtcMillis();
            return IntervalOp.Evaluate(start, end, evaluateParams);
        }
    }
}
