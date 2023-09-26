///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public interface ExprEventEvaluator
    {
        object Eval(
            EventBean @event,
            ExprEvaluatorContext ctx);
    }

    public class ProxyExprEventEvaluator : ExprEventEvaluator
    {
        public Func<EventBean, ExprEvaluatorContext, object> ProcEval { get; set; }

        public ProxyExprEventEvaluator()
        {
        }

        public ProxyExprEventEvaluator(Func<EventBean, ExprEvaluatorContext, object> procEval)
        {
            ProcEval = procEval;
        }

        public object Eval(
            EventBean @event,
            ExprEvaluatorContext ctx)
        {
            return ProcEval.Invoke(@event, ctx);
        }
    }
} // end of namespace