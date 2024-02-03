///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsNodeForgeNCEvalIsArrayObject : ExprEqualsNodeForgeNCEvalBase
    {
        public ExprEqualsNodeForgeNCEvalIsArrayObject(
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
            var left = (Array)Lhs.Evaluate(eventsPerStream, isNewData, context);
            var right = (Array)Rhs.Evaluate(eventsPerStream, isNewData, context);

            bool result;
            if (left == null) {
                result = right == null;
            }
            else {
                result = right != null && Arrays.DeepEquals(left, right);
            }

            result = result ^ parent.IsNotEquals;

            return result;
        }
    }
} // end of namespace