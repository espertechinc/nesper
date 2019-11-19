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

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationTAAReaderLinearWindowForge : AggregationTableAccessAggReaderForge
    {
        private readonly ExprNode optionalEvaluator;

        public AggregationTAAReaderLinearWindowForge(
            Type arrayType,
            ExprNode optionalEvaluator)
        {
            ResultType = arrayType;
            this.optionalEvaluator = optionalEvaluator;
        }

        public Type ResultType { get; }

        public CodegenExpression CodegenCreateReader(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationTAAReaderLinearWindow), GetType(), classScope);
            method.Block
                .DeclareVar<AggregationTAAReaderLinearWindow>(
                    "strat",
                    NewInstance(typeof(AggregationTAAReaderLinearWindow)))
                .SetProperty(Ref("strat"), "ComponentType", Constant(ResultType.GetElementType()))
                .SetProperty(
                    Ref("strat"),
                    "OptionalEvaluator",
                    optionalEvaluator == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            optionalEvaluator.Forge,
                            method,
                            GetType(),
                            classScope))
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace