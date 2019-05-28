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

namespace com.espertech.esper.common.@internal.epl.agg.access.core
{
    public class AggregationAgentDefaultWFilterForge : AggregationAgentForge
    {
        private readonly ExprForge filterEval;

        public AggregationAgentDefaultWFilterForge(ExprForge filterEval)
        {
            this.filterEval = filterEval;
        }

        public CodegenExpression Make(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance<AggregationAgentDefaultWFilter>(
                ExprNodeUtilityCodegen.CodegenEvaluator(filterEval, method, this.GetType(), classScope));
        }

        public ExprForge FilterEval {
            get => filterEval;
        }

        public ExprForge OptionalFilter {
            get => filterEval;
        }
    }
} // end of namespace