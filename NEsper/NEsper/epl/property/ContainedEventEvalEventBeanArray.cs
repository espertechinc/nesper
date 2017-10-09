///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.property
{
    public class ContainedEventEvalEventBeanArray : ContainedEventEval
    {
        private readonly ExprEvaluator _evaluator;

        public ContainedEventEvalEventBeanArray(ExprEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public Object GetFragment(
            EventBean eventBean,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return _evaluator.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
        }
    }
} // end of namespace
