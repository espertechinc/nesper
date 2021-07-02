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
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
    public class AggregationAgentCountMinSketchForge : AggregationAgentForge
    {
        private readonly ExprForge _stringEvaluator;
        private readonly ExprForge _optionalFilterForge;

        public AggregationAgentCountMinSketchForge(
            ExprForge stringEvaluator,
            ExprForge optionalFilterForge)
        {
            this._stringEvaluator = stringEvaluator;
            this._optionalFilterForge = optionalFilterForge;
        }

        public ExprForge OptionalFilter {
            get => _optionalFilterForge;
        }

        public CodegenExpression Make(
            CodegenMethod parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationAgentCountMinSketch), GetType(), classScope);
            method.Block
                .DeclareVarNewInstance<AggregationAgentCountMinSketch>("cms")
                .SetProperty(
                    Ref("cms"),
                    "StringEval",
                    ExprNodeUtilityCodegen.CodegenEvaluator(_stringEvaluator, method, GetType(), classScope))
                .SetProperty(
                    Ref("cms"),
                    "OptionalFilterEval",
                    _optionalFilterForge == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            _optionalFilterForge,
                            method,
                            GetType(),
                            classScope))
                .MethodReturn(Ref("cms"));
            return LocalMethod(method);
        }
    }
} // end of namespace