///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.method.sum;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.method.avg
{
    /// <summary>
    ///     Average that generates double-typed numbers.
    /// </summary>
    public class AggregatorAvgNumeric : AggregatorSumNumeric
    {
        private readonly AggregationForgeFactoryAvg _factoryMethodAvg;

        public AggregatorAvgNumeric(
            AggregationForgeFactoryAvg factory,
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter,
            Type sumType)
            : base(optionalDistinctValueType, optionalDistinctSerde, hasFilter, optionalFilter, sumType)
        {
            _factoryMethodAvg = factory;
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block
                .IfCondition(EqualsIdentity(cnt, Constant(0)))
                .BlockReturn(ConstantNull());

            // Optional match context comes into play for floating point numerics
            CodegenExpressionInstanceField mathContext = null;
            if (_factoryMethodAvg.OptionalMathContext != null) {
                mathContext = classScope.AddOrGetDefaultFieldSharable(
                    new MathContextCodegenField(_factoryMethodAvg.OptionalMathContext));
            }

            var sumTypeBoxed = sumType.GetBoxedType();
            if (sumTypeBoxed == typeof(decimal?)) {
                if (mathContext == null) {
                    method.Block.MethodReturn(Op(sum, "/", cnt));
                }
                else {
                    method.Block.MethodReturn(
                        StaticMethod(
                            typeof(MathContextExtensions),
                            "GetValueDivide",
                            mathContext,
                            sum,
                            cnt));
                }
            }
            else if (sumTypeBoxed == typeof(double?)) {
                if (mathContext == null) {
                    method.Block.MethodReturn(Op(sum, "/", cnt));
                }
                else {
                    method.Block.MethodReturn(
                        StaticMethod(
                            typeof(MathContextExtensions),
                            "GetValueDivide",
                            mathContext,
                            sum,
                            cnt));
                }
            }
            else if (mathContext == null) {
                method.Block.MethodReturn(Op(sum, "/", Cast<double>(cnt)));
            }
            else {
                method.Block.MethodReturn(
                    StaticMethod(
                        typeof(MathContextExtensions),
                        "GetValueDivide",
                        mathContext,
                        sum,
                        Cast<double>(cnt)));
            }
        }
    }
} // end of namespace