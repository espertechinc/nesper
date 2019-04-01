///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public abstract class ScriptEvaluatorBase : ScriptEvaluator
    {
        internal readonly SimpleNumberCoercer coercer;
        internal readonly string[] parameterNames;
        internal readonly ExprEvaluator[] parameters;

        internal readonly string scriptName;

        public ScriptEvaluatorBase(
            string scriptName,
            string[] parameterNames,
            ExprEvaluator[] parameters,
            SimpleNumberCoercer coercer)
        {
            this.scriptName = scriptName;
            this.parameterNames = parameterNames;
            this.parameters = parameters;
            this.coercer = coercer;
        }

        public abstract object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        public abstract object Evaluate(
            object lookupValues,
            ExprEvaluatorContext exprEvaluatorContext);

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var result = Evaluate(eventsPerStream, isNewData, context);
            return ScriptResultToROCollectionEvents(result);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var result = Evaluate(eventsPerStream, isNewData, context);
            if (result == null) {
                return null;
            }

            return result.Unwrap<object>();
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        private static ICollection<EventBean> ScriptResultToROCollectionEvents(object result)
        {
            if (result == null) {
                return null;
            }

            if (result.GetType().IsArray) {
                return (EventBean[]) result;
            }

            return (ICollection<EventBean>) result;
        }
    }
} // end of namespace