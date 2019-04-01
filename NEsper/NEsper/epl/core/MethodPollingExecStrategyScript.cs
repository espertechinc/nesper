///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.db;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.script;

namespace com.espertech.esper.epl.core
{
    public class MethodPollingExecStrategyScript : PollExecStrategy
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ExprNodeScriptEvaluator _eval;

        public MethodPollingExecStrategyScript(ExprNodeScript scriptExpression, EventType eventTypeEventBeanArray)
        {
            _eval = (ExprNodeScriptEvaluator) scriptExpression.ExprEvaluator;
        }

        public void Start()
        {
        }

        public IList<EventBean> Poll(Object[] lookupValues, ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = _eval.Evaluate(lookupValues, exprEvaluatorContext);
            if (!(result is EventBean[]))
            {
                Log.Warn(
                    "Script expected return type EventBean[] does not match result {0}",
                    (result == null ? "null" : result.GetType().FullName));
                return Collections.GetEmptyList<EventBean>();
            }
            return result.UnwrapIntoList<EventBean>();
        }

        public void Done()
        {
        }

        public void Dispose()
        {
        }
    }
} // end of namespace
