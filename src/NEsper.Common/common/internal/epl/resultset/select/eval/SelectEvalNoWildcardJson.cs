///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalNoWildcardJson : SelectExprProcessorForge
    {
        private readonly SelectExprForgeContext selectContext;
        private readonly JsonEventType jsonEventType;

        public SelectEvalNoWildcardJson(
            SelectExprForgeContext selectContext,
            JsonEventType jsonEventType)
        {
            this.selectContext = selectContext;
            this.jsonEventType = jsonEventType;
        }

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
            methodNode.Block.DeclareVar(
                jsonEventType.UnderlyingType,
                "und",
                NewInstanceInner(jsonEventType.Detail.UnderlyingClassName));
            for (var i = 0; i < selectContext.ColumnNames.Length; i++) {
                var columnName = selectContext.ColumnNames[i];
                var fieldClassBoxed =
                    jsonEventType.Detail.FieldDescriptors.Get(columnName).PropertyType.GetBoxedType();
                var propertyType = jsonEventType.Types.Get(columnName);
                var evalType = selectContext.ExprForges[i].EvaluationType;
                var field = jsonEventType.Detail.FieldDescriptors.Get(selectContext.ColumnNames[i]);
                CodegenExpression rhs = null;
                // handle
                if (typeof(EventBean).Equals(evalType)) {
                    var conversion = methodNode.MakeChild(fieldClassBoxed, GetType(), codegenClassScope)
                        .AddParam<object>("value");
                    conversion.Block.IfRefNullReturnNull("value")
                        .MethodReturn(
                            Cast(
                                fieldClassBoxed,
                                ExprDotMethod(Cast(typeof(EventBean), Ref("value")), "getUnderlying")));
                    rhs = LocalMethod(
                        conversion,
                        CodegenLegoMayVoid.ExpressionMayVoid(
                            typeof(EventBean),
                            selectContext.ExprForges[i],
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
                }
                else if (propertyType is Type) {
                    rhs = CodegenLegoMayVoid.ExpressionMayVoid(
                        fieldClassBoxed,
                        selectContext.ExprForges[i],
                        methodNode,
                        exprSymbol,
                        codegenClassScope);
                }
                else if (propertyType is TypeBeanOrUnderlying) {
                    var underlyingType = ((TypeBeanOrUnderlying)propertyType).EventType.UnderlyingType;
                    var conversion = methodNode.MakeChild(underlyingType, GetType(), codegenClassScope)
                        .AddParam<object>("value");
                    conversion.Block.IfRefNullReturnNull("value")
                        .IfInstanceOf("value", typeof(EventBean))
                        .BlockReturn(Cast(underlyingType, ExprDotUnderlying(Cast(typeof(EventBean), Ref("value")))))
                        .MethodReturn(Cast(underlyingType, Ref("value")));
                    rhs = LocalMethod(
                        conversion,
                        CodegenLegoMayVoid.ExpressionMayVoid(
                            typeof(object),
                            selectContext.ExprForges[i],
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
                }
                else if (propertyType is TypeBeanOrUnderlying[]) {
                    var underlyingType = ((TypeBeanOrUnderlying[])propertyType)[0].EventType.UnderlyingType;
                    var underlyingArrayType = TypeHelper.GetArrayType(underlyingType);
                    var conversion = methodNode.MakeChild(underlyingArrayType, GetType(), codegenClassScope)
                        .AddParam<object>("value");
                    conversion.Block.IfRefNullReturnNull("value")
                        .IfInstanceOf("value", underlyingArrayType)
                        .BlockReturn(Cast(underlyingArrayType, Ref("value")))
                        .DeclareVar<EventBean[]>("events", Cast(typeof(EventBean[]), Ref("value")))
                        .DeclareVar(
                            underlyingArrayType,
                            "array",
                            NewArrayByLength(underlyingType, ArrayLength(Ref("events"))))
                        .ForLoopIntSimple("i", ArrayLength(Ref("events")))
                        .AssignArrayElement(
                            "array",
                            Ref("i"),
                            Cast(underlyingType, ExprDotUnderlying(ArrayAtIndex(Ref("events"), Ref("i")))))
                        .BlockEnd()
                        .MethodReturn(Ref("array"));
                    rhs = LocalMethod(
                        conversion,
                        CodegenLegoMayVoid.ExpressionMayVoid(
                            typeof(object),
                            selectContext.ExprForges[i],
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
                }
                else if (propertyType == null) {
                    methodNode.Block.Expression(
                        CodegenLegoMayVoid.ExpressionMayVoid(
                            typeof(object),
                            selectContext.ExprForges[i],
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
                }
                else {
                    throw new UnsupportedOperationException("Unrecognized property ");
                }

                if (rhs != null) {
                    if (field.PropertyType.IsPrimitive) {
                        var tmp = "result_" + i;
                        methodNode.Block.DeclareVar(fieldClassBoxed, tmp, rhs)
                            .IfRefNotNull(tmp)
                            .AssignRef(ExprDotName(Ref("und"), field.FieldName), Ref(tmp))
                            .BlockEnd();
                    }
                    else {
                        methodNode.Block.AssignRef(ExprDotName(Ref("und"), field.FieldName), rhs);
                    }
                }
            }

            methodNode.Block.MethodReturn(
                ExprDotMethod(eventBeanFactory, "adapterForTypedJson", Ref("und"), resultEventType));
            return methodNode;
        }

        public EventType ResultEventType => jsonEventType;
    }
} // end of namespace