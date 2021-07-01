///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.updatehelper
{
    public class EventBeanUpdateItemForge
    {
        public EventBeanUpdateItemForge(
            ExprForge expression,
            string optionalPropertyName,
            EventPropertyWriterSPI optionalWriter,
            bool notNullableField,
            TypeWidenerSPI optionalWidener,
            bool useUntypedAssignment,
            bool useTriggeringEvent,
            EventBeanUpdateItemArray optionalArray)
        {
            Expression = expression;
            OptionalPropertyName = optionalPropertyName;
            OptionalWriter = optionalWriter;
            IsNotNullableField = notNullableField;
            OptionalWidener = optionalWidener;
            IsUseUntypedAssignment = useUntypedAssignment;
            IsUseTriggeringEvent = useTriggeringEvent;
            OptionalArray = optionalArray;
        }

        public ExprForge Expression { get; }

        public string OptionalPropertyName { get; }

        public EventPropertyWriterSPI OptionalWriter { get; }

        public bool IsNotNullableField { get; }

        public TypeWidenerSPI OptionalWidener { get; }

        public EventBeanUpdateItemArray OptionalArray { get; }

        public bool IsUseUntypedAssignment { get; }

        public bool IsUseTriggeringEvent { get; }


        public EventBeanUpdateItemForgeWExpressions ToExpression(
            Type type,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var rhs = Expression.EvaluateCodegen(type, parent, symbols, classScope);
            EventBeanUpdateItemArrayExpressions arrayExpressions = null;
            if (OptionalArray != null) {
                arrayExpressions = OptionalArray.GetArrayExpressions(parent, symbols, classScope);
            }

            return new EventBeanUpdateItemForgeWExpressions(rhs, arrayExpressions);
        }
    }
} // end of namespace