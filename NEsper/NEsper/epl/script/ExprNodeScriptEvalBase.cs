///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.script
{
    [Serializable]
    public abstract class ExprNodeScriptEvalBase
        : ExprEvaluator
        , ExprEvaluatorEnumeration
    {
        protected readonly String ScriptName;
        protected readonly String StatementName;
        protected readonly String[] Names;
        protected readonly ExprEvaluator[] Parameters;
        private readonly Type _returnType;
        protected readonly EventType EventTypeCollection;
        protected readonly Coercer Coercer;

        protected ExprNodeScriptEvalBase(String scriptName, String statementName, String[] names, ExprEvaluator[] parameters, Type returnType, EventType eventTypeCollection)
        {
            ScriptName = scriptName;
            StatementName = statementName;
            Names = names;
            Parameters = parameters;
            _returnType = returnType;
            EventTypeCollection = eventTypeCollection;

            if (returnType.IsNumeric())
            {
                Coercer = CoercerFactory.GetCoercer(returnType.GetBoxedType());
            }
            else
            {
                Coercer = null;
            }
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }

        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
            return EventTypeCollection;
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var result = Evaluate(new EvaluateParams(eventsPerStream, isNewData, context));
            if (result == null)
            {
                return null;
            }

            return result.Unwrap<EventBean>();
        }

        public Type ComponentTypeCollection
        {
            get
            {
                if (_returnType.IsArray)
                {
                    return _returnType.GetElementType();
                }
                return null;
            }
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var result = Evaluate(new EvaluateParams(eventsPerStream, isNewData, context));
            if (result == null)
            {
                return null;
            }

            if (result is ICollection<object>)
            {
                return (ICollection<object>)result;
            }

            if (result is Array)
            {
                return ((Array)result).Cast<object>().ToList();
            }

            throw new ArgumentException("invalid result type returned from evaluate; expected array");
        }

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public abstract object Evaluate(EvaluateParams evaluateParams);
    }
}
