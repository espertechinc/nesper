///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public interface ExprDotEval
    {
        object Evaluate(object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);

        ExprDotForge DotForge { get; }
    }

    public class ProxyExprDotEval : ExprDotEval
    {
        public Func<object, EventBean[], bool, ExprEvaluatorContext, object> ProcEvaluate;
        public Func<ExprDotForge> ProcDotForge;

        public object Evaluate(
            object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
            => ProcEvaluate?.Invoke(target, eventsPerStream, isNewData, exprEvaluatorContext);

        public ExprDotForge DotForge
            => ProcDotForge?.Invoke();
    }
} // end of namespace