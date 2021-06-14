///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotForgeGetArray : ExprDotForge
    {
        public ExprDotForgeGetArray(
            ExprForge index,
            Type componentType)
        {
            IndexExpression = index;
            TypeInfo = EPTypeHelper.SingleValue(componentType);
        }

        public ExprForge IndexExpression { get; }

        public EPType TypeInfo { get; }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitArraySingleItemSource();
        }

        public ExprDotEval DotEvaluator => new ExprDotForgeGetArrayEval(this, IndexExpression.ExprEvaluator);

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return ExprDotForgeGetArrayEval.Codegen(this, inner, innerType, parent, symbols, classScope);
        }
    }
} // end of namespace