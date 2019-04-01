///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.updatehelper
{
    public class EventBeanUpdateItem
    {
        public EventBeanUpdateItem(
            ExprEvaluator expression,
            string optinalPropertyName,
            EventPropertyWriter optionalWriter,
            bool notNullableField,
            TypeWidener optionalWidener)
        {
            Expression = expression;
            OptionalPropertyName = optinalPropertyName;
            OptionalWriter = optionalWriter;
            IsNotNullableField = notNullableField;
            OptionalWidener = optionalWidener;
        }

        public ExprEvaluator Expression { get; private set; }

        public string OptionalPropertyName { get; private set; }

        public EventPropertyWriter OptionalWriter { get; private set; }

        public bool IsNotNullableField { get; private set; }

        public TypeWidener OptionalWidener { get; private set; }
    }
}