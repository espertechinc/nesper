///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public abstract class ExprEqualsNodeForgeNCEvalBase : ExprEvaluator
    {
        protected readonly ExprEvaluator Lhs;
        protected readonly ExprEqualsNodeImpl parent;
        protected readonly ExprEvaluator Rhs;

        protected ExprEqualsNodeForgeNCEvalBase(
            ExprEqualsNodeImpl parent,
            ExprEvaluator lhs,
            ExprEvaluator rhs)
        {
            this.parent = parent;
            Lhs = lhs;
            Rhs = rhs;
        }

        public abstract object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);
    }
} // end of namespace