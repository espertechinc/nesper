///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.updatehelper
{
    public class EventBeanUpdateItemForge
    {
        public EventBeanUpdateItemForge(
            ExprForge expression, string optinalPropertyName, EventPropertyWriterSPI optionalWriter,
            bool notNullableField, TypeWidenerSPI optionalWidener)
        {
            Expression = expression;
            OptionalPropertyName = optinalPropertyName;
            OptionalWriter = optionalWriter;
            IsNotNullableField = notNullableField;
            OptionalWidener = optionalWidener;
        }

        public ExprForge Expression { get; }

        public string OptionalPropertyName { get; }

        public EventPropertyWriterSPI OptionalWriter { get; }

        public bool IsNotNullableField { get; }

        public TypeWidenerSPI OptionalWidener { get; }
    }
} // end of namespace