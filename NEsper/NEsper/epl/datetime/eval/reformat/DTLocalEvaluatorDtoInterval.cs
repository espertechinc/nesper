///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.epl.datetime.interval;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.datetime.eval.reformat
{
    internal class DTLocalEvaluatorDtoInterval : DTLocalEvaluatorIntervalBase
    {
        internal DTLocalEvaluatorDtoInterval(IntervalOp intervalOp)
            : base(intervalOp)
        {
        }

        public override object Evaluate(object target, EvaluateParams evaluateParams)
        {
            var time = ((DateTimeOffset)target).TimeInMillis();
            return IntervalOp.Evaluate(time, time, evaluateParams);
        }

        public override object Evaluate(object startTimestamp, object endTimestamp, EvaluateParams evaluateParams)
        {
            var start = ((DateTimeOffset)startTimestamp).TimeInMillis();
            var end = ((DateTimeOffset)endTimestamp).TimeInMillis();
            return IntervalOp.Evaluate(start, end, evaluateParams);
        }
    }
}
