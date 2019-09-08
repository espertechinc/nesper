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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.typable
{
    public class SelectExprProcessorTypableMapEval : ExprEvaluator
    {
        private readonly SelectExprProcessorTypableMapForge forge;

        public SelectExprProcessorTypableMapEval(SelectExprProcessorTypableMapForge forge)
        {
            this.forge = forge;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException("Evaluate not supported");
        }

        public static CodegenExpression Codegen(
            SelectExprProcessorTypableMapForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression mapType = codegenClassScope.AddFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(forge.MapType, EPStatementInitServicesConstants.REF));
            CodegenExpression beanFactory =
                codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);

            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                typeof(SelectExprProcessorTypableMapEval),
                codegenClassScope);

            methodNode.Block
                .DeclareVar<IDictionary<string, object>>(
                    "values",
                    forge.innerForge.EvaluateCodegen(
                        typeof(IDictionary<string, object>),
                        methodNode,
                        exprSymbol,
                        codegenClassScope))
                .DeclareVarNoInit(typeof(IDictionary<string, object>), "map")
                .IfRefNull("values")
                .AssignRef("values", StaticMethod(typeof(Collections), "GetEmptyMap", new[] { typeof(string), typeof(object) }))
                .BlockEnd()
                .MethodReturn(ExprDotMethod(beanFactory, "AdapterForTypedMap", @Ref("values"), mapType));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace