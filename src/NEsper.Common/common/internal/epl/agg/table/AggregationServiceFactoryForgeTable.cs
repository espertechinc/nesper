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
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.table
{
    public class AggregationServiceFactoryForgeTable : AggregationServiceFactoryForgeWProviderGen
    {
        private readonly TableMetaData metadata;
        private readonly TableColumnMethodPairForge[] methodPairs;
        private readonly int[] accessColumnsZeroOffset;
        private readonly AggregationAgentForge[] accessAgents;
        private readonly AggregationGroupByRollupDescForge groupByRollupDesc;

        public AggregationServiceFactoryForgeTable(
            TableMetaData metadata,
            TableColumnMethodPairForge[] methodPairs,
            int[] accessColumnsZeroOffset,
            AggregationAgentForge[] accessAgents,
            AggregationGroupByRollupDescForge groupByRollupDesc)
        {
            this.metadata = metadata;
            this.methodPairs = methodPairs;
            this.accessColumnsZeroOffset = accessColumnsZeroOffset;
            this.accessAgents = accessAgents;
            this.groupByRollupDesc = groupByRollupDesc;
        }

        public CodegenExpression MakeProvider(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(AggregationServiceFactoryTable), GetType(), classScope);
            method.Block
                .DeclareVar<AggregationServiceFactoryTable>(
                    "factory",
                    NewInstance(typeof(AggregationServiceFactoryTable)))
                .SetProperty(
                    Ref("factory"),
                    "Table",
                    TableDeployTimeResolver.MakeResolveTable(metadata, symbols.GetAddInitSvc(method)))
                .SetProperty(
                    Ref("factory"),
                    "MethodPairs",
                    TableColumnMethodPairForge.MakeArray(methodPairs, method, symbols, classScope))
                .SetProperty(Ref("factory"), "AccessColumnsZeroOffset", Constant(accessColumnsZeroOffset))
                .SetProperty(
                    Ref("factory"),
                    "AccessAgents",
                    AggregationAgentUtil.MakeArray(accessAgents, method, symbols, classScope))
                .SetProperty(
                    Ref("factory"),
                    "GroupByRollupDesc",
                    groupByRollupDesc == null ? ConstantNull() : groupByRollupDesc.Codegen(method, classScope))
                .MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }
    }
} // end of namespace