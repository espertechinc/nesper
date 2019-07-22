///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    /// <summary>
    ///     Processor for select-clause expressions that handles wildcards. Computes results based on matching events.
    /// </summary>
    public class SelectEvalJoinWildcardProcessorObjectArray : SelectExprProcessorForge
    {
        private readonly string[] streamNames;

        public SelectEvalJoinWildcardProcessorObjectArray(
            string[] streamNames,
            EventType resultEventType)
        {
            this.streamNames = streamNames;
            ResultEventType = resultEventType;
        }

        public EventType ResultEventType { get; }

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventTypeOuter,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            // NOTE: Maintaining result-event-type as out own field as we may be an "inner" select-expr-processor
            var mType = codegenClassScope.AddFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(ResultEventType, EPStatementInitServicesConstants.REF));
            var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
            var refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .DeclareVar<object[]>("tuple", NewArrayByLength(typeof(object), Constant(streamNames.Length)))
                .StaticMethod(
                    typeof(Array),
                    "Copy",
                    refEPS,
                    Constant(0),
                    Ref("tuple"),
                    Constant(0),
                    Constant(streamNames.Length))
                .MethodReturn(ExprDotMethod(eventBeanFactory, "AdapterForTypedObjectArray", Ref("tuple"), mType));
            return methodNode;
        }
    }
} // end of namespace