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
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.inner
{
    public class InnerDotScalarUnpackEventForge : ExprDotEvalRootChildInnerForge
    {
        private ExprForge rootForge;

        public InnerDotScalarUnpackEventForge(ExprForge rootForge)
        {
            this.rootForge = rootForge;
        }

        public ExprDotEvalRootChildInnerEval InnerEvaluator {
            get => new InnerDotScalarUnpackEventEval(rootForge.ExprEvaluator);
        }

        public CodegenExpression CodegenEvaluate(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return InnerDotScalarUnpackEventEval.CodegenEvaluate(this, parentMethod, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public EventType EventTypeCollection {
            get => null;
        }

        public Type ComponentTypeCollection {
            get => null;
        }

        public EventType EventTypeSingle {
            get => null;
        }

        public EPType TypeInfo {
            get => EPTypeHelper.SingleValue(rootForge.EvaluationType);
        }

        public ExprForge RootForge {
            get => rootForge;
        }
    }
} // end of namespace