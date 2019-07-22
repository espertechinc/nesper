///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprConcatNodeEvalWNew : ExprEvaluator
    {
        private readonly ExprEvaluator[] evaluators;
        private readonly ExprConcatNode parent;

        public ExprConcatNodeEvalWNew(
            ExprConcatNode parent,
            ExprEvaluator[] evaluators)
        {
            this.parent = parent;
            this.evaluators = evaluators;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var buffer = new StringBuilder();
            return Evaluate(eventsPerStream, isNewData, context, buffer, evaluators, parent);
        }

        protected internal static string Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context,
            StringBuilder buffer,
            ExprEvaluator[] evaluators,
            ExprConcatNode parent)
        {
            foreach (var child in evaluators) {
                var result = (string) child.Evaluate(eventsPerStream, isNewData, context);
                if (result == null) {
                    return null;
                }

                buffer.Append(result);
            }

            return buffer.ToString();
        }
    }
} // end of namespace