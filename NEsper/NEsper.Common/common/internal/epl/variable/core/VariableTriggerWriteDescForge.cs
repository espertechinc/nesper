///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    public class VariableTriggerWriteDescForge
    {
        private readonly Type evaluationType;
        private readonly EventPropertyGetterSPI getter;
        private readonly Type getterType;

        public VariableTriggerWriteDescForge(
            EventTypeSPI type, string variableName, EventPropertyWriterSPI writer, EventPropertyGetterSPI getter,
            Type getterType, Type evaluationType)
        {
            Type = type;
            VariableName = variableName;
            Writer = writer;
            this.getter = getter;
            this.getterType = getterType;
            this.evaluationType = evaluationType;
        }

        public string VariableName { get; }

        public EventPropertyWriterSPI Writer { get; }

        public EventTypeSPI Type { get; }

        public EventPropertyValueGetterForge Getter => getter;

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(VariableTriggerWriteDesc), GetType(), classScope);
            method.Block
                .DeclareVar(typeof(VariableTriggerWriteDesc), "desc", NewInstance(typeof(VariableTriggerWriteDesc)))
                .ExprDotMethod(
                    Ref("desc"), "setType", EventTypeUtility.ResolveTypeCodegen(Type, symbols.GetAddInitSvc(method)))
                .ExprDotMethod(Ref("desc"), "setVariableName", Constant(VariableName))
                .ExprDotMethod(
                    Ref("desc"), "setWriter",
                    EventTypeUtility.CodegenWriter(
                        Type, getterType, evaluationType, Writer, method, GetType(), classScope))
                .ExprDotMethod(
                    Ref("desc"), "setGetter",
                    EventTypeUtility.CodegenGetterWCoerce(getter, getterType, null, method, GetType(), classScope))
                .MethodReturn(Ref("desc"));
            return LocalMethod(method);
        }
    }
} // end of namespace