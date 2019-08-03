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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalEnumerationSingleToCollForge : ExprForge
    {
        internal readonly ExprEnumerationForge enumerationForge;
        private readonly EventType targetType;

        public ExprEvalEnumerationSingleToCollForge(
            ExprEnumerationForge enumerationForge,
            EventType targetType)
        {
            this.enumerationForge = enumerationForge;
            this.targetType = targetType;
        }

        public ExprEvaluator ExprEvaluator {
            get { throw ExprNodeUtilityMake.MakeUnsupportedCompileTime(); }
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean[]),
                typeof(ExprEvalEnumerationSingleToCollForge),
                codegenClassScope);

            methodNode.Block
                .DeclareVar<EventBean>(
                    "@event",
                    enumerationForge.EvaluateGetEventBeanCodegen(methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("@event")
                .DeclareVar<EventBean[]>("events", NewArrayByLength(typeof(EventBean), Constant(1)))
                .AssignArrayElement(@Ref("events"), Constant(0), @Ref("@event"))
                .MethodReturn(@Ref("events"));
            return LocalMethod(methodNode);
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public Type EvaluationType {
            get => TypeHelper.GetArrayType(targetType.UnderlyingType);
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => enumerationForge.EnumForgeRenderable;
        }
    }
} // end of namespace