///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.client.hook.forgeinject;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.plugin
{
    public class AggregatorPlugInManaged : AggregatorMethodWDistinctWFilterWValueBase
    {
        private CodegenExpressionMember plugin;
        private readonly AggregationFunctionModeManaged mode;

        public AggregatorPlugInManaged(
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter,
            AggregationFunctionModeManaged mode) : base(
            optionalDistinctValueType,
            optionalDistinctSerde,
            hasFilter,
            optionalFilter)
        {
            this.mode = mode;
        }

        public override void InitForgeFiltered(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            var injectionStrategy = (InjectionStrategyClassNewInstance)mode.InjectionStrategyAggregationFunctionFactory;
            var factoryField = classScope.AddDefaultFieldUnshared(
                true,
                typeof(AggregationFunctionFactory),
                injectionStrategy.GetInitializationExpression(classScope));

            plugin = membersColumnized.AddMember(col, typeof(AggregationFunction), "plugin");
            rowCtor.Block.AssignRef(plugin, ExprDotMethod(factoryField, "NewAggregator", ConstantNull()));
        }

        protected override void ApplyEvalEnterNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(plugin, "Enter", value);
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(plugin, "Leave", value);
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(plugin, "Enter", value);
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(plugin, "Leave", value);
        }

        protected override void ClearWODistinct(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(plugin, "Clear");
        }

        protected override void WriteWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef output,
            CodegenExpressionRef unitKey,
            CodegenExpressionRef writer,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (mode.HasHA) {
                method.Block.StaticMethod(mode.Serde, "write", output, RowDotMember(row, plugin));
            }
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenExpressionRef unitKey,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (mode.HasHA) {
                method.Block.AssignRef(RowDotMember(row, plugin), StaticMethod(mode.Serde, "read", input));
            }
        }

        protected override void AppendFormatWODistinct(FabricTypeCollector collector)
        {
            if (mode.HasHA) {
                collector.PlugInAggregation(mode.Serde);
            }
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(ExprDotName(plugin, "Value"));
        }
    }
} // end of namespace