///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.historical.method.poll;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.method.core
{
    public class MethodPollingViewableMeta
    {
        public MethodPollingViewableMeta(
            Type methodProviderClass,
            bool isStaticMethod,
            IDictionary<string, object> optionalMapType,
            LinkedHashMap<string, object> optionalOaType,
            MethodPollingExecStrategyEnum strategy,
            bool isCollection,
            bool isIterator,
            VariableMetaData variable,
            EventType eventTypeEventBeanArray,
            ExprNodeScript scriptExpression)
        {
            MethodProviderClass = methodProviderClass;
            IsStaticMethod = isStaticMethod;
            OptionalMapType = optionalMapType;
            OptionalOaType = optionalOaType;
            Strategy = strategy;
            IsCollection = isCollection;
            IsIterator = isIterator;
            Variable = variable;
            EventTypeEventBeanArray = eventTypeEventBeanArray;
            ScriptExpression = scriptExpression;
        }

        public IDictionary<string, object> OptionalMapType { get; }

        public LinkedHashMap<string, object> OptionalOaType { get; }

        public MethodPollingExecStrategyEnum Strategy { get; }

        public bool IsCollection { get; }

        public bool IsIterator { get; }

        public VariableMetaData Variable { get; }

        public EventType EventTypeEventBeanArray { get; }

        public ExprNodeScript ScriptExpression { get; }

        public Type MethodProviderClass { get; }

        public bool IsStaticMethod { get; }
    }
} // end of namespace