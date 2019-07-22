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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
    public class AgregationTAAReaderCountMinSketchFreqForge : AggregationTableAccessAggReaderForge
    {
        private readonly ExprNode frequencyEval;

        public AgregationTAAReaderCountMinSketchFreqForge(ExprNode frequencyEval)
        {
            this.frequencyEval = frequencyEval;
        }

        public Type ResultType => typeof(long);

        public CodegenExpression CodegenCreateReader(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AgregationTAAReaderCountMinSketchFreq), GetType(), classScope);
            method.Block
                .DeclareVar<AgregationTAAReaderCountMinSketchFreq>(
                    "strat",
                    NewInstance(typeof(AgregationTAAReaderCountMinSketchFreq)))
                .SetProperty(
                    Ref("strat"),
                    "FrequencyEval",
                    ExprNodeUtilityCodegen.CodegenEvaluator(frequencyEval.Forge, method, GetType(), classScope))
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace