///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.contained
{
    public class ContainedEventEvalEventBeanArray : ContainedEventEval
    {
        private readonly ExprEvaluator evaluator;

        public ContainedEventEvalEventBeanArray(ExprEvaluator evaluator)
        {
            this.evaluator = evaluator;
        }

        public object GetFragment(
            EventBean eventBean,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);
        }
    }
} // end of namespace