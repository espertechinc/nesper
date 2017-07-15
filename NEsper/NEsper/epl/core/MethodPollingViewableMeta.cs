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
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.epl.core
{
    public class MethodPollingViewableMeta
    {
        public MethodPollingViewableMeta(
            Type methodProviderClass,
            bool isStaticMethod,
            IDictionary<string, Object> optionalMapType,
            IDictionary<string, Object> optionalOaType,
            Object invocationTarget,
            MethodPollingExecStrategyEnum strategy,
            bool isCollection,
            bool isIterator,
            VariableReader variableReader,
            string variableName,
            EventType eventTypeEventBeanArray,
            ExprNodeScript scriptExpression)
        {
            MethodProviderClass = methodProviderClass;
            IsStaticMethod = isStaticMethod;
            OptionalMapType = optionalMapType;
            OptionalOaType = optionalOaType;
            InvocationTarget = invocationTarget;
            Strategy = strategy;
            IsCollection = isCollection;
            IsEnumerator = isIterator;
            VariableReader = variableReader;
            VariableName = variableName;
            EventTypeEventBeanArray = eventTypeEventBeanArray;
            ScriptExpression = scriptExpression;
        }

        public IDictionary<string, object> OptionalMapType { get; private set; }

        public IDictionary<string, object> OptionalOaType { get; private set; }

        public object InvocationTarget { get; private set; }

        public MethodPollingExecStrategyEnum Strategy { get; private set; }

        public bool IsCollection { get; private set; }

        public bool IsEnumerator { get; private set; }

        public VariableReader VariableReader { get; private set; }

        public string VariableName { get; private set; }

        public EventType EventTypeEventBeanArray { get; private set; }

        public ExprNodeScript ScriptExpression { get; private set; }

        public Type MethodProviderClass { get; private set; }

        public bool IsStaticMethod { get; private set; }
    }
} // end of namespace
