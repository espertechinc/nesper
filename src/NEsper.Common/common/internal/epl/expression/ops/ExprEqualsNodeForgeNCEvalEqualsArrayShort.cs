///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsNodeForgeNCEvalEqualsArrayShort : ExprEqualsNodeForgeNCEvalBase
    {
        public ExprEqualsNodeForgeNCEvalEqualsArrayShort(
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
            var left = (short[])Lhs.Evaluate(eventsPerStream, isNewData, context);
            var right = (short[])Rhs.Evaluate(eventsPerStream, isNewData, context);

            if (left == null || right == null) { // null comparison
                return null;
            }

            return Arrays.AreEqual(left, right) ^ parent.IsNotEquals;
        }
    }
} // end of namespace