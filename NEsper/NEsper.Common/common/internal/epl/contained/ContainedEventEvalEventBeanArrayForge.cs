///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.contained
{
    public class ContainedEventEvalEventBeanArrayForge : ContainedEventEvalForge
    {
        private readonly ExprForge evaluator;

        public ContainedEventEvalEventBeanArrayForge(ExprForge evaluator)
        {
            this.evaluator = evaluator;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContainedEventEvalEventBeanArray), GetType(), classScope);
            method.Block.MethodReturn(
                NewInstance<ContainedEventEvalEventBeanArray>(
                    ExprNodeUtilityCodegen.CodegenEvaluator(evaluator, method, GetType(), classScope)));
            return LocalMethod(method);
        }
    }
} // end of namespace