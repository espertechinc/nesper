///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class AggregationMethodLinearWindowForge : AggregationMethodForge
    {
        private readonly Type arrayType;
        private readonly ExprNode optionalEvaluator;

        public AggregationMethodLinearWindowForge(
            Type arrayType,
            ExprNode optionalEvaluator)
        {
            this.arrayType = arrayType;
            this.optionalEvaluator = optionalEvaluator;
        }

        public CodegenExpression CodegenCreateReader(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationMethodLinearWindow), GetType(), classScope);
            method.Block
                .DeclareVar<AggregationMethodLinearWindow>("strat", NewInstance(typeof(AggregationMethodLinearWindow)))
                .SetProperty(Ref("strat"), "ComponentType", Constant(arrayType.GetElementType()))
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

        public Type ResultType => arrayType;
    }
} // end of namespace