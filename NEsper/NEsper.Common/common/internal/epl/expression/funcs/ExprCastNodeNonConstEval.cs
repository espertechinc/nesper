///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCastNodeNonConstEval : ExprEvaluator
    {
        private readonly ExprCastNode parent;
        private readonly ExprEvaluator evaluator;
        private readonly ExprCastNode.CasterParserComputer casterParserComputer;

        public ExprCastNodeNonConstEval(
            ExprCastNode parent,
            ExprEvaluator evaluator,
            ExprCastNode.CasterParserComputer casterParserComputer)
        {
            this.parent = parent;
            this.evaluator = evaluator;
            this.casterParserComputer = casterParserComputer;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            object result = evaluator.Evaluate(eventsPerStream, isNewData, context);
            if (result != null) {
                result = casterParserComputer.Compute(result, eventsPerStream, isNewData, context);
            }

            return result;
        }
    }
} // end of namespace