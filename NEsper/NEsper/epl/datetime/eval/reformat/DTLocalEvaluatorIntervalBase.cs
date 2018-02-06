///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.datetime.interval;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.datetime.eval.reformat
{
    internal abstract class DTLocalEvaluatorIntervalBase
        : DTLocalEvaluator
        , DTLocalEvaluatorIntervalComp
    {
        protected readonly IntervalOp IntervalOp;

        protected DTLocalEvaluatorIntervalBase(IntervalOp intervalOp)
        {
            IntervalOp = intervalOp;
        }

        public abstract object Evaluate(object target, EvaluateParams evaluateParams);
        public abstract object Evaluate(object startTimestamp, object endTimestamp, EvaluateParams evaluateParams);
    }
}
