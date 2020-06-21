///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsNodeForgeNCEvalEqualsNonArray : ExprEqualsNodeForgeNCEvalBase
    {
        public ExprEqualsNodeForgeNCEvalEqualsNonArray(
            ExprEqualsNodeImpl parent,
            ExprEvaluator lhs,
            ExprEvaluator rhs) : base(parent, lhs, rhs)
        {
        }

        public override object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var left = Lhs.Evaluate(eventsPerStream, isNewData, context);
            var right = Rhs.Evaluate(eventsPerStream, isNewData, context);

            if (left == null || right == null) { // null comparison
                return null;
            }

            return left.Equals(right) ^ parent.IsNotEquals;
        }
    }
} // end of namespace