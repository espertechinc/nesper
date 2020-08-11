///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
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
    public class AggregatorAvgNonBig : AggregatorSumNonBig
    {
        private readonly AggregationForgeFactory _factoryMethodAvg;

        public AggregatorAvgNonBig(
            AggregationForgeFactory factory,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter,
            Type sumType)
            : base(factory, col, rowCtor, membersColumnized, classScope, optionalDistinctValueType, optionalDistinctSerde, hasFilter, optionalFilter, sumType)
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

            var sumTypeBoxed = sumType.GetBoxedType();
            if (sumTypeBoxed == typeof(decimal?)) {
                method.Block.MethodReturn(Op(sum, "/", cnt));
            }
            else if (sumTypeBoxed == typeof(double?)) {
                method.Block.MethodReturn(Op(sum, "/", cnt));
            }
            else {
                method.Block.MethodReturn(Op(sum, "/", Cast<double>(cnt)));
            }
        }
    }
} // end of namespace