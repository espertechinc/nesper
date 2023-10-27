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
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalEnumerationEventBeanToUnderlyingArrayForge : ExprForge
    {
        protected readonly ExprEnumerationForge enumerationForge;
        private readonly EventType targetType;

        public ExprEvalEnumerationEventBeanToUnderlyingArrayForge(
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
                EvaluationType,
                typeof(ExprEvalEnumerationEventBeanToUnderlyingArrayForge),
                codegenClassScope);
            methodNode.Block
                .DeclareVar<EventBean>("@event",
                    enumerationForge.EvaluateGetEventBeanCodegen(methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("@event")
                .DeclareVar(EvaluationType, "array", NewArrayByLength(targetType.UnderlyingType, Constant(1)))
                .AssignArrayElement(
                    Ref("array"),
                    Constant(0),
                    Cast(targetType.UnderlyingType, ExprDotUnderlying(Ref("@event"))))
                .MethodReturn(Ref("array"));
            return LocalMethod(methodNode);
        }

        public ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public Type EvaluationType => TypeHelper.GetArrayType(targetType.UnderlyingType);

        public ExprNodeRenderable ExprForgeRenderable => enumerationForge.EnumForgeRenderable;
    }
} // end of namespace