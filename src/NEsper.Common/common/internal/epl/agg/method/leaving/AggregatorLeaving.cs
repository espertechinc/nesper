///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.fabric;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.leaving
{
    public class AggregatorLeaving : AggregatorMethod
    {
        private readonly AggregationForgeFactoryLeaving factory;
        private CodegenExpressionMember leaving;

        public AggregatorLeaving(AggregationForgeFactoryLeaving factory)
        {
            this.factory = factory;
        }

        public void InitForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            leaving = membersColumnized.AddMember(col, typeof(bool), "leaving");
        }

        public void ApplyEvalEnterCodegen(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
        }

        public void ApplyEvalLeaveCodegen(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            if (factory.AggregationExpression.PositionalParams.Length > 0) {
                PrefixWithFilterCheck(
                    factory.AggregationExpression.PositionalParams[0].Forge,
                    method,
                    symbols,
                    classScope);
            }

            method.Block.AssignRef(leaving, ConstantTrue());
        }

        public void ApplyTableEnterCodegen(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
        }

        public void ApplyTableLeaveCodegen(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(leaving, ConstantTrue());
        }

        public void ClearCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(leaving, ConstantFalse());
        }

        public void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(leaving);
        }

        public void WriteCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef output,
            CodegenExpressionRef unitKey,
            CodegenExpressionRef writer,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Apply(WriteBoolean(output, row, leaving));
        }

        public void ReadCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenExpressionRef unitKey,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Apply(ReadBoolean(row, leaving, input));
        }

        public void CollectFabricType(FabricTypeCollector collector)
        {
            collector.Builtin(typeof(bool));
        }
    }
} // end of namespace