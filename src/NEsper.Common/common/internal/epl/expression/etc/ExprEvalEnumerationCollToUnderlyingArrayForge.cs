///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalEnumerationCollToUnderlyingArrayForge : ExprForge
    {
        protected readonly ExprEnumerationForge enumerationForge;
        private readonly EventType targetType;

        public ExprEvalEnumerationCollToUnderlyingArrayForge(
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
                typeof(ExprEvalEnumerationCollToUnderlyingArrayForge),
                codegenClassScope);
            methodNode.Block
                .DeclareVar<ICollection<EventBean>>(
                    "events",
                    enumerationForge.EvaluateGetROCollectionEventsCodegen(methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("events")
                .DeclareVar(
                    TypeHelper.GetArrayType(targetType.UnderlyingType),
                    "array",
                    NewArrayByLength(targetType.UnderlyingType, ExprDotName(Ref("events"), "Count")))
                .DeclareVar<IEnumerator<EventBean>>(
                    "en",
                    ExprDotMethod(Ref("events"), "GetEnumerator"))
                .DeclareVar<int>("index", Constant(0))
                .WhileLoop(ExprDotMethod(Ref("en"), "MoveNext"))
                .DeclareVar<EventBean>("@event", Cast(typeof(EventBean), ExprDotName(Ref("en"), "Current")))
                .AssignArrayElement(
                    "array",
                    Ref("index"),
                    Cast(targetType.UnderlyingType, ExprDotUnderlying(Ref("@event"))))
                .IncrementRef("index")
                .BlockEnd()
                .MethodReturn(Ref("array"));
            return LocalMethod(methodNode);
        }

        public ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public Type EvaluationType => TypeHelper.GetArrayType(targetType.UnderlyingType);

        public ExprNodeRenderable ExprForgeRenderable => enumerationForge.EnumForgeRenderable;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;
    }
} // end of namespace