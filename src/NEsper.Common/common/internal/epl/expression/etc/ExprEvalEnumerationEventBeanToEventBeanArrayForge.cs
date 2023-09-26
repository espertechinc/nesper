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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.typable;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalEnumerationEventBeanToEventBeanArrayForge : ExprForge,
        SelectExprProcessorTypableForge
    {
        protected readonly ExprEnumerationForge enumerationForge;
        private readonly EventType targetType;

        public ExprEvalEnumerationEventBeanToEventBeanArrayForge(
            ExprEnumerationForge enumerationForge,
            EventType targetType)
        {
            this.enumerationForge = enumerationForge;
            this.targetType = targetType;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean[]),
                typeof(ExprEvalEnumerationEventBeanToEventBeanArrayForge),
                codegenClassScope);
            methodNode.Block
                .DeclareVar<EventBean>("event",
                    enumerationForge.EvaluateGetEventBeanCodegen(methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("event")
                .DeclareVar(typeof(EventBean[]), "events", NewArrayByLength(typeof(EventBean), Constant(1)))
                .AssignArrayElement(Ref("events"), Constant(0), Ref("event"))
                .MethodReturn(Ref("events"));
            return LocalMethod(methodNode);
        }

        public ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public Type EvaluationType => typeof(EventBean[]);

        public Type UnderlyingEvaluationType => TypeHelper.GetArrayType(targetType.UnderlyingType);

        public ExprNodeRenderable ExprForgeRenderable => enumerationForge.EnumForgeRenderable;
    }
} // end of namespace