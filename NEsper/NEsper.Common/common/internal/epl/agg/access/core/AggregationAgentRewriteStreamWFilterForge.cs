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
    public class AggregationAgentRewriteStreamWFilterForge : AggregationAgentForge
    {
        private readonly int _streamNum;
        private readonly ExprForge _filterEval;

        public AggregationAgentRewriteStreamWFilterForge(
            int streamNum,
            ExprForge filterEval)
        {
            _streamNum = streamNum;
            _filterEval = filterEval;
        }

        public CodegenExpression Make(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance<AggregationAgentRewriteStreamWFilter>(
                Constant(_streamNum),
                ExprNodeUtilityCodegen.CodegenEvaluator(_filterEval, method, this.GetType(), classScope));
        }

        public ExprForge FilterEval
        {
            get => _filterEval;
        }

        public ExprForge OptionalFilter
        {
            get => _filterEval;
        }
    }
} // end of namespace