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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotForgeArrayGet : ExprDotForge
    {
        private readonly EPType typeInfo;
        private readonly ExprForge indexExpression;

        public ExprDotForgeArrayGet(
            ExprForge index,
            Type componentType)
        {
            this.indexExpression = index;
            this.typeInfo = EPTypeHelper.SingleValue(componentType);
        }

        public EPType TypeInfo {
            get => typeInfo;
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitArraySingleItemSource();
        }

        public ExprDotEval DotEvaluator {
            get => new ExprDotForgeArrayGetEval(this, indexExpression.ExprEvaluator);
        }

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotForgeArrayGetEval.Codegen(
                this,
                inner,
                innerType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public ExprForge IndexExpression {
            get => indexExpression;
        }
    }
} // end of namespace