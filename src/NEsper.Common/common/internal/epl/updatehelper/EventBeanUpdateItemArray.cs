///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.updatehelper
{
    public class EventBeanUpdateItemArray
    {
        public EventBeanUpdateItemArray(
            string propertyName,
            ExprNode indexExpression,
            Type arrayType,
            EventPropertyGetterSPI getter)
        {
            PropertyName = propertyName;
            IndexExpression = indexExpression;
            ArrayType = arrayType;
            Getter = getter;
        }

        public string PropertyName { get; }

        public ExprNode IndexExpression { get; }

        public Type ArrayType { get; }

        public EventPropertyGetterSPI Getter { get; }

        public EventBeanUpdateItemArrayExpressions GetArrayExpressions(
            CodegenMethodScope parentScope,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var index = IndexExpression.Forge.EvaluateCodegen(typeof(int?), parentScope, symbols, classScope);
            var arrayGet = EvaluateArrayCodegen(parentScope, symbols, classScope);
            return new EventBeanUpdateItemArrayExpressions(index, arrayGet);
        }

        private CodegenExpression EvaluateArrayCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(ArrayType, GetType(), classScope);
            method.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(symbols.GetAddEps(method), Constant(0)))
                .IfRefNullReturnNull("@event")
                .MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        ArrayType,
                        Getter.EventBeanGetCodegen(Ref("@event"), method, classScope)));
            return LocalMethod(method);
        }
    }
} // end of namespace